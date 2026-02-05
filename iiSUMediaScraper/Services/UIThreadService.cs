using iiSUMediaScraper.Contracts.Services;
using Microsoft.UI.Dispatching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iiSUMediaScraper.Services;

/// <summary>
/// Provides functionality to dispatch actions to the UI thread from background threads.
/// </summary>
public class UIThreadService : IUIThreadService
{
    /// <summary>
    /// Dispatches an action to run on the UI thread.
    /// </summary>
    /// <param name="action">The action to execute on the UI thread.</param>
    /// <returns>A completed task.</returns>
    public Task DispachToUIThread(Action action)
    {
        DispatcherQueue?.TryEnqueue(() => action());

        return Task.CompletedTask;
    }

    /// <summary>
    /// Gets or sets the dispatcher queue for the UI thread.
    /// </summary>
    public static DispatcherQueue? DispatcherQueue { get; set; }
}
