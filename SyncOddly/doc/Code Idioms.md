# SyncOddly Code Idioms

## Comparing to MongoDB/Realm templates

### Commands
Note that the current Realm official templates use [RelayCommand from MVVMToolkit][RC]

We use the idiom of declaring a public Command then assigning it in the ViewModel constructor, eg for a parameterised command

```
    SharedWithTapped = new Command<SharedWith>(OnSharedWithTapped);
invoking
    async void OnSharedWithTapped(SharedWith SharedWith)
```    


[RC]: https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/generators/relaycommand
