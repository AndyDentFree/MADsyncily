using System;
namespace SyncOddly.ViewModels;

/// <summary>
/// Use when you want to provide implementations for OnAppearing and OnDisappearing
/// invoked by a BasePage
/// </summary>
public interface AppearanceDetectingVM
{
    void OnAppearing();
    void OnDisappearing();
}

