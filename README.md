# iiSU Media Scraper

A powerful WinUI 3 application for scraping, managing, and formatting game media assets from multiple sources. Designed to help organize and optimize game icons, titles, logos, hero images, and slide images for gaming platforms and frontends.

## Features

### Multi-Source Media Scraping
- **SteamGridDB Integration** - Access thousands of high-quality game assets
- **IGDB Integration** - Scrape game covers, screenshots, and videos with OAuth authentication
- **IGN Integration** - Fetch game screenshots and media from IGN's extensive database

### Intelligent Media Management
- **Smart Cropping** - Automatic intelligent cropping for icons, logos, titles, heroes, and slides
- **Cropping** - Cropping for icons, logos, titles, heroes, and slides
- **Image Upscaling** - Integration with SpeedVR2 to enhance low-resolution images
- **Drag & Drop Support** - Easy media import with intuitive drag-and-drop zones

### Platform Configuration
- **Multi-Platform Support** - Configure settings for various gaming platforms
- **Custom Platform Definitions** - Define platform codes, names, and translations
- **Folder Mapping** - Automatic organization by platform and game
- **File Extension Mapping** - Associate file extensions with specific platforms
- **Platform Icon Overlays** - Add custom overlays to game icons per platform

### Batch Processing
- **Concurrent Scraping** - Process multiple games simultaneously with configurable concurrency
- **Progress Tracking** - Real-time progress monitoring for scraping and applying operations
- **Selective Processing** - Filter and process specific games or platforms

### Advanced Image Editing
- **Image Cropper** - Cropping for precise adjustments of icons, logos, titles, heroes, and slides
- **Aspect Ratio Control** - Lock aspect ratios for consistent media dimensions
- **Preview Mode** - Preview media before applying changes
- **Manual Adjustments** - Fine-tune crop regions and settings per image

### Configuration Management
- **Searchable Settings** - Quickly find and filter configuration options
- **Scraper Configuration** - Configure API keys and settings for each media source
- **Output Formatting** - Define output dimensions, naming patterns, and file formats

## Requirements

- Windows 10 version 1809 (build 17763) or later
- Windows 11 recommended
- .NET 8.0 Runtime
- **Python (non-Windows Store version)** - Required for SpeedVR2 image upscaling
- Internet connection for media scraping
- **API Keys Required** - SteamGridDB and IGDB accounts with API keys needed for media scraping (see API Keys section below)

## Installation

1. Download the latest release from the [Releases](../../releases) page
2. Extract the ZIP file to a folder of your choice
3. Run `iiSUMediaScraper.exe`
4. Configure your API keys in the Configuration section (see Configuration below)

## Getting Started

### Initial Setup

1. **Configure Paths**
   - Set your games folder path where game files are located
   - Set assets folder path where formatted media should be saved

2. **Configure Scrapers**
   - **SteamGridDB**: Enter your API key from [SteamGridDB Settings](https://www.steamgriddb.com/profile/preferences/api)
   - **IGDB**: Configure Client ID and Client Secret from [Twitch Developer Console](https://dev.twitch.tv/console/apps)
   - **IGN**: No configuration required

3. **Set Up Platforms**
   - Add platform definitions (e.g., Nintendo Switch, PlayStation 5, PC)
   - Configure folder names for each platform
   - Define file extensions for each platform (e.g., .nsp, .xci for Switch)

### Basic Workflow

1. **Begin Scraping**
   - Click "Begin" to scan your games folder
   - The application will find games based on your folder and extension configurations

2. **Review Results**
   - Browse through platforms and games
   - Preview scraped media (icons, titles, logos, heroes, slides)
   - Select preferred media for each game if needed

3. **Edit & Adjust**
   - Crop images to desired dimensions
   - Upscale low-resolution images

4. **Apply Media**
   - Click the save button on the left to save all platforms and game assets to your assets folder
   - Click the save button on the top to save all game assets for a particual platform to your assets folder
   - Click the save button on the game to save game specific assets

## Configuration

### Platform Setup

Configure platforms in the **Platforms** tab:
- **Code**: Short identifier (e.g., "NS", "PS5")
- **Name**: Full platform name
- **Translation**: Alternative names for better matching

### Folder Configurations

Map folder names to platforms:
- Folder name → Platform code
- Used for automatic platform detection

### Extension Configurations

Define file extensions per platform:
- Platform code → List of extensions
- Example: Nintendo Switch → [.nsp, .xci, .nsz]

### Scraper Settings

Configure media scrapers:
- Enable/disable specific scrapers
- Set API keys and authentication
- Configure rate limits and timeouts

### Image Upscaler Settings

Configure [SpeedVR2](https://github.com/AbdullahBRashid/SpeedVR2) for image upscaling:
- **Python Requirement**: Python (non-Windows Store version) must be installed and accessible from PATH
- **SpeedVR2**: Fast video/image upscaling tool used by this application
- Arguments

### Output Formatting

Define output settings:
- **Icon**: Dimensions and naming format
- **Title**: Dimensions and naming format
- **Logo**: Dimensions and naming format
- **Hero**: Dimensions and naming format
- **Slide**: Dimensions and naming format

### Advanced Options

- **Max Concurrent Games**: Number of games to process simultaneously
- **Scan Games**: Enable scanning of main games folder
- **Scan Unfound Games**: Enable scanning of unfound games folder
- **Apply Unfound Games**: Include games without media in apply operations
- **Move to Unfound Folder**: Automatically move games without media

## Technologies

- **WinUI 3** - Modern Windows UI framework
- **MVVM Architecture** - Clean separation of concerns
- **.NET 8.0** - Latest .NET runtime
- **CommunityToolkit.Mvvm** - MVVM helpers and generators
- **ImageMagick** - Advanced image processing
- **SpeedVR2** - Fast image upscaling
- **HttpClient** - HTTP communication with rate limiting
- **OAuth 2.0** - Secure API authentication

## API Keys

**REQUIRED**: This application requires API keys and accounts for SteamGridDB and IGDB to function. Media scraping will not work without proper API credentials configured.

### SteamGridDB (Required)
1. Visit [SteamGridDB](https://www.steamgriddb.com/)
2. Create an account or log in
3. Go to [Settings → API](https://www.steamgriddb.com/profile/preferences/api)
4. Generate an API key
5. Enter the API key in Configuration → Scrapers → SteamGridDB

### IGDB (Required - via Twitch)
1. Visit [Twitch Developer Console](https://dev.twitch.tv/console/apps)
2. Register a new application
   - Name: Choose any name (e.g., "iiSU Media Scraper")
   - OAuth Redirect URLs: `http://localhost` (or leave default)
   - Category: Choose "Application Integration"
3. Copy the **Client ID** and **Client Secret**
4. Enter both credentials in Configuration → Scrapers → IGDB

**Note**: IGN scraper does not require API keys and works without authentication.

## License

GPL

## Support

For issues, questions, or suggestions, please open an issue on GitHub.

---

**Note**: This application is not affiliated with or endorsed by iiSU, SteamGridDB, IGDB, IGN, or any gaming platform mentioned.
