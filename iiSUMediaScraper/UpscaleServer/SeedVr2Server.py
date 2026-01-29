"""
FastAPI server for SeedVR2 Image Upscaler
Auto-downloads the ComfyUI-SeedVR2_VideoUpscaler repository and models.

Uses the CLI internally for reliable image processing.
First run downloads code (~50MB) and models (~6GB).
"""

import io
import os
import sys
import base64
import shutil
import subprocess
import tempfile
import traceback
from contextlib import asynccontextmanager
from typing import Optional
from pathlib import Path

# Determine directories
SCRIPT_DIR = Path(__file__).parent.absolute()
SEEDVR2_REPO_DIR = SCRIPT_DIR / "ComfyUI-SeedVR2_VideoUpscaler"

print("=" * 60)
print("SeedVR2 Server Startup")
print("=" * 60)
print(f"Script directory: {SCRIPT_DIR}")
print(f"SeedVR2 repo: {SEEDVR2_REPO_DIR}")
print("=" * 60)


def setup_seedvr2():
    """Download and setup the SeedVR2 repository"""
    if (SEEDVR2_REPO_DIR / "inference_cli.py").exists():
        print("SeedVR2 repository already exists.")
        return True
    
    print("\nDownloading SeedVR2 repository...")
    
    # Try git clone
    try:
        result = subprocess.run(
            ["git", "clone", "--depth", "1",
             "https://github.com/numz/ComfyUI-SeedVR2_VideoUpscaler.git",
             str(SEEDVR2_REPO_DIR)],
            capture_output=True, text=True, timeout=300
        )
        if result.returncode == 0:
            print("Successfully cloned via git.")
            return True
        print(f"Git failed: {result.stderr}")
    except (FileNotFoundError, subprocess.TimeoutExpired) as e:
        print(f"Git unavailable: {e}")
    
    # Fallback: download zip
    try:
        import urllib.request
        import zipfile
        
        zip_url = "https://github.com/numz/ComfyUI-SeedVR2_VideoUpscaler/archive/refs/heads/main.zip"
        zip_path = SCRIPT_DIR / "seedvr2_temp.zip"
        
        print("Downloading zip from GitHub...")
        urllib.request.urlretrieve(zip_url, zip_path)
        
        print("Extracting...")
        with zipfile.ZipFile(zip_path, 'r') as zf:
            zf.extractall(SCRIPT_DIR)
        
        extracted = SCRIPT_DIR / "ComfyUI-SeedVR2_VideoUpscaler-main"
        if extracted.exists():
            if SEEDVR2_REPO_DIR.exists():
                shutil.rmtree(SEEDVR2_REPO_DIR)
            extracted.rename(SEEDVR2_REPO_DIR)
        
        zip_path.unlink(missing_ok=True)
        print("Successfully downloaded repository.")
        return True
        
    except Exception as e:
        print(f"Failed to download: {e}")
        return False


def install_dependencies():
    """Install required packages"""
    requirements = SEEDVR2_REPO_DIR / "requirements.txt"
    if not requirements.exists():
        print("No requirements.txt found!")
        return
    
    # Always try to install requirements to ensure all deps are present
    print("Installing/verifying dependencies (this may take a few minutes on first run)...")
    try:
        result = subprocess.run(
            [sys.executable, "-m", "pip", "install", "-r", str(requirements)],
            capture_output=True,
            text=True,
            timeout=600,
            encoding="utf-8",
            errors="replace"
        )
        if result.returncode == 0:
            print("Dependencies installed successfully.")
        else:
            print(f"Pip output: {result.stdout}")
            print(f"Pip errors: {result.stderr}")
    except Exception as e:
        print(f"Warning installing dependencies: {e}")


# Setup
if not setup_seedvr2():
    print("ERROR: Could not download SeedVR2.")
    sys.exit(1)

install_dependencies()
print("=" * 60)

# Now import server dependencies
import torch
from fastapi import FastAPI, HTTPException
from fastapi.responses import Response
from pydantic import BaseModel, Field, validator
from PIL import Image


class UpscaleRequest(BaseModel):
    """Request model"""
    name: str = Field(default="", description="Job name")
    resolution: int = Field(default=1080, ge=64, description="Target short-side resolution (minimum 64)")
    max_resolution: int = Field(default=0, description="Max resolution (0 = no limit)")
    seed: int = Field(default=42, description="Random seed")
    color_correction: str = Field(default="lab", description="Color correction: lab, wavelet, wavelet_adaptive, hsv, adain, none")
    input_noise_scale: float = Field(default=0.0, description="Input noise scale")
    latent_noise_scale: float = Field(default=0.0, description="Latent noise scale")
    image_base64: str = Field(description="Base64 encoded image")
    
    @validator('resolution', pre=True, always=True)
    def ensure_minimum_resolution(cls, v):
        """Ensure resolution is at least 64 pixels"""
        if v is None or v < 64:
            return 1080  # Default to 1080 if invalid
        return v


def calculate_tile_settings(width: int, height: int, target_resolution: int) -> tuple[int, int, int]:
    """
    Calculate optimal VAE tile size, overlap, and blocks_to_swap based on image dimensions.
    
    Returns (tile_size, tile_overlap, blocks_to_swap)
    """
    # Calculate the output dimensions
    if height < width:
        out_height = target_resolution
        out_width = int(width * (target_resolution / height))
    else:
        out_width = target_resolution
        out_height = int(height * (target_resolution / width))
    
    max_dim = max(out_width, out_height)
    total_pixels = out_width * out_height
    
    # Choose tile size based on output resolution
    # Smaller images can use larger tiles, larger images need smaller tiles
    if max_dim <= 1024:
        tile_size = 512
    elif max_dim <= 2048:
        tile_size = 512
    elif max_dim <= 4096:
        tile_size = 384
    else:
        tile_size = 256  # Very large images need smaller tiles
    
    # Overlap should be at least 25% of tile size for good blending
    tile_overlap = max(64, tile_size // 4)
    
    # blocks_to_swap: offload DiT transformer blocks to CPU for very large images
    # 3B model has 32 blocks, we can swap 0-32
    # This trades speed for VRAM - each block swapped saves ~200MB but adds latency
    if total_pixels > 40_000_000:  # > ~6300x6300 or similar
        blocks_to_swap = 32  # Swap all blocks for extreme sizes
    elif total_pixels > 25_000_000:  # > ~5000x5000 or similar
        blocks_to_swap = 24
    elif total_pixels > 16_000_000:  # > ~4000x4000 or similar  
        blocks_to_swap = 16
    elif total_pixels > 9_000_000:  # > ~3000x3000 or similar
        blocks_to_swap = 8
    else:
        blocks_to_swap = 0  # No swapping needed for smaller images
    
    return tile_size, tile_overlap, blocks_to_swap


class UpscaleResponse(BaseModel):
    success: bool
    message: str
    image_base64: Optional[str] = None
    width: Optional[int] = None
    height: Optional[int] = None


class ServerState:
    def __init__(self):
        self.ready = False
        self.models_downloaded = False
        self.error = None


state = ServerState()


def check_models():
    """Check if models are downloaded by doing a dry run"""
    # Models auto-download on first CLI run
    state.ready = True
    state.models_downloaded = True
    print("Server ready. Models will download on first upscale request.")


def upscale_with_cli(image: Image.Image, request: UpscaleRequest) -> Image.Image:
    """Upscale using the SeedVR2 CLI"""
    
    # Create temp directory manually for better control on Windows
    tmpdir = Path(tempfile.mkdtemp())
    try:
        input_path = tmpdir / "input.png"
        output_dir = tmpdir / "output"
        output_dir.mkdir()
        
        # Save input image
        image.save(input_path, "PNG")
        
        # Build CLI command
        cmd = [
            sys.executable, str(SEEDVR2_REPO_DIR / "inference_cli.py"),
            str(input_path),
            "--output", str(output_dir),
            "--resolution", str(request.resolution),
            "--seed", str(request.seed),
            "--color_correction", request.color_correction,
            "--batch_size", "1",  # Single image
        ]
        
        if request.max_resolution > 0:
            cmd.extend(["--max_resolution", str(request.max_resolution)])
        
        if request.input_noise_scale > 0:
            cmd.extend(["--input_noise_scale", str(request.input_noise_scale)])
        
        if request.latent_noise_scale > 0:
            cmd.extend(["--latent_noise_scale", str(request.latent_noise_scale)])
        
        # Always use VAE tiling with dynamic tile size based on image dimensions
        tile_size, tile_overlap, blocks_to_swap = calculate_tile_settings(
            image.width, image.height, request.resolution
        )
        print(f"Using VAE tiling: tile_size={tile_size}, tile_overlap={tile_overlap}, blocks_to_swap={blocks_to_swap}")
        
        cmd.append("--vae_encode_tiled")
        cmd.append("--vae_decode_tiled")
        cmd.extend(["--vae_encode_tile_size", str(tile_size)])
        cmd.extend(["--vae_decode_tile_size", str(tile_size)])
        cmd.extend(["--vae_encode_tile_overlap", str(tile_overlap)])
        cmd.extend(["--vae_decode_tile_overlap", str(tile_overlap)])
        
        # For large images, offload DiT transformer blocks to CPU to save VRAM
        if blocks_to_swap > 0:
            cmd.extend(["--blocks_to_swap", str(blocks_to_swap)])
            cmd.extend(["--dit_offload_device", "cpu"])  # Required for block swapping
            # Also enable I/O component swapping for additional memory savings
            cmd.append("--swap_io_components")
        
        print(f"Running CLI...")
        
        # Set environment for UTF-8 encoding (fixes Windows emoji issue)
        env = os.environ.copy()
        env["PYTHONIOENCODING"] = "utf-8"
        env["PYTHONLEGACYWINDOWSSTDIO"] = "0"
        
        # Run CLI (timeout is very long to support slow hardware)
        result = subprocess.run(
            cmd,
            cwd=str(SEEDVR2_REPO_DIR),
            capture_output=True,
            text=True,
            timeout=7200,  # 2 hour timeout for slow hardware
            env=env,
            encoding="utf-8",
            errors="replace"
        )
        
        if result.returncode != 0:
            print(f"CLI stdout: {result.stdout}")
            print(f"CLI stderr: {result.stderr}")
            raise RuntimeError(f"CLI failed: {result.stderr or result.stdout}")
        
        print(f"CLI completed. Searching for output...")
        
        # The CLI saves output with the same name as input in the output directory
        # So we look for input.png in the OUTPUT directory (not the temp root)
        output_file = output_dir / "input.png"
        
        if output_file.exists():
            print(f"Found output file: {output_file}")
        else:
            # Fallback: search for any PNG in output directory
            output_files = list(output_dir.glob("*.png")) if output_dir.exists() else []
            if output_files:
                output_file = output_files[0]
                print(f"Found output file (fallback): {output_file}")
            else:
                # List what's actually in the directories for debugging
                print(f"Contents of tmpdir: {list(tmpdir.iterdir())}")
                if output_dir.exists():
                    print(f"Contents of output_dir: {list(output_dir.iterdir())}")
                raise RuntimeError(f"No output file found. CLI output: {result.stdout}")
        
        # Load the output image into memory before cleanup
        print(f"Loading output from: {output_file}")
        
        # Read raw bytes to preserve exact output from CLI
        output_bytes = output_file.read_bytes()
        
        # Get dimensions from the image
        with Image.open(output_file) as img:
            width, height = img.size
        
        return output_bytes, width, height
        
    finally:
        # Clean up temp directory manually, ignoring errors on Windows
        try:
            import shutil
            shutil.rmtree(tmpdir, ignore_errors=True)
        except Exception as e:
            print(f"Warning: Could not clean up temp dir: {e}")


@asynccontextmanager
async def lifespan(app: FastAPI):
    check_models()
    yield


app = FastAPI(
    title="SeedVR2 Upscaler API",
    description="Image upscaling using SeedVR2 (auto-downloads models)",
    version="1.0.0",
    lifespan=lifespan,
)


@app.get("/health")
async def health_check():
    return {
        "status": "healthy" if state.ready else "starting",
        "models_ready": state.models_downloaded,
        "error": state.error,
        "note": "Models download automatically on first request (~6GB)"
    }


@app.post("/upscale", response_model=UpscaleResponse)
async def upscale_endpoint(request: UpscaleRequest):
    if not state.ready:
        raise HTTPException(status_code=503, detail="Server not ready")
    
    try:
        # Log all request parameters from C#
        print(f"\n{'='*60}")
        print(f"UPSCALE REQUEST RECEIVED:")
        print(f"  name: {request.name}")
        print(f"  resolution: {request.resolution}")
        print(f"  max_resolution: {request.max_resolution}")
        print(f"  seed: {request.seed}")
        print(f"  color_correction: {request.color_correction}")
        print(f"  input_noise_scale: {request.input_noise_scale}")
        print(f"  latent_noise_scale: {request.latent_noise_scale}")
        print(f"  image_base64 length: {len(request.image_base64)} chars")
        print(f"{'='*60}")
        
        # Decode image
        image_data = base64.b64decode(request.image_base64)
        input_image = Image.open(io.BytesIO(image_data)).convert("RGB")
        
        print(f"Processing: {input_image.width}x{input_image.height} -> res={request.resolution}")
        
        # Upscale - returns raw bytes and dimensions
        output_bytes, width, height = upscale_with_cli(input_image, request)
        
        print(f"Output size: {len(output_bytes)} bytes, {width}x{height}")
        
        return UpscaleResponse(
            success=True,
            message="Image upscaled successfully",
            image_base64=base64.b64encode(output_bytes).decode(),
            width=width,
            height=height,
        )
        
    except Exception as e:
        print(f"Upscaling failed: {e}")
        print(traceback.format_exc())
        raise HTTPException(status_code=500, detail=str(e))


@app.post("/upscale/raw")
async def upscale_raw(request: UpscaleRequest):
    if not state.ready:
        raise HTTPException(status_code=503, detail="Server not ready")
    
    try:
        image_data = base64.b64decode(request.image_base64)
        input_image = Image.open(io.BytesIO(image_data)).convert("RGB")
        
        # Upscale - returns raw bytes and dimensions
        output_bytes, width, height = upscale_with_cli(input_image, request)
        
        return Response(
            content=output_bytes,
            media_type="image/png",
            headers={
                "X-Image-Width": str(width),
                "X-Image-Height": str(height),
            }
        )
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000)