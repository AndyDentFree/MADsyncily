# SyncOddly - Xamarin Forms Shell pattern with Realm Sync

Testing Realm sync patterns within conventional Xamarin Forms apps, for more challenging and realistic data models typical with shared data.

This sample was designed to push beyond the normal trivial cases, so you can explore nuances of realistic data volumes and editing patterns.

There will be a [companion article][ADonM] exploring these ideas in more detail, released soon after this repo goes public.

This is a first draft being put out for public comment on the architecture and may well get significant changes. An [issue's been created][Comments] for feedback including recommendations of other samples to look at.

Note: well after I'd started work on this sample, the [MAUI Sample][NewTut] was released as part of `realm-cli` and it considerably improves on its predecessors, with very clean code including using [MVVM Toolkit][MVT]. However, it's still using a much simpler model of data than this sample and as such doesn't inform some of the sharing idioms tackled here.

See further docs such as:

- `./doc/Architecture.md`
- `./doc/Subscriptions.md`
- `./doc/SyncOddlyFAQ.md`
- `./doc/SyncOddly UX Design.md`

You may also note in the diaries that thinking and work was stretched over more than a year, as a side-project and hoping for more clarity in patterns. The 2023 release of the official [MAUI sample][NewTut] helped. However, we explore a few consequences of sharing more data than it covers.

## Testing mode and volume caution
Whilst showing patterns for sync, the apps are also designed as testbeds for how well sync performs.

That includes being able to select different users within the app and also to generate sample data. 

**Be careful with your MongoDB account if you build a sync version and generate a lot of data.**


## Background
This project evolved from an initial consideration of partition key (PK) strategies, to focus on how to get the most from FlexibleSync (FS). I needed to explore FS for an old REST-based app I've been migrating to Xamarin Forms, which needs to be offline-first.

In that app, we have users who share _some_ items with friends. 

### Prejudices and Wonderings
I don't usually clutter up sample code with this kind of stuff but it's been a sticking point for me on this sample and a related project. Other people may too have these issues so I decided to come out and say things we often won't. I _don't have authoritative answers_ and delivering a working version of SyncOddly is just the start to explore this.

**In general across the .Net culture, at least as seen in Xamarin Forms, I see a heavier amount of _ceremony_, to the point where a starting blank app has many data abstractions.**

Particularly for a Realm-oriented app, it's hard to tell if these are:

1. Industry norms that should be complied with, at least to make common teams comfortable.
2. Useful abstractions.
3. Actively harmful abstractions, blocking use of Realm features or causing sync problems.
4. All of the above.

See the _Uncertainties__ section at end.

## General Realm Xamarin Feature use in SyncOddly
Simple [(archived) realm-tutorial-dotnet][RTUT] and [MAUI template][NewTut] used to provide some guidance.

### Data types used in SyncOddly
- `ObjectID` for keys
- simple primitives `string` and `int`
- `DateTimeOffset`
- `IList<SomeEmbeddedObjectType>` to have owned lists in `SharedWith` and `Person`

Currently avoiding types (just to keep things simple):
- enums
- dictionaries
- relationships? (although the sample in [Quick start with Sync][QSdoc] uses them)

### Schema Creation
Following the pattern for specific classes only, from the [Advanced Guides - Schema][schema] doc.



## SyncOddly Test app notes
Created using Visual Studio 8.10.23 as a Shell app in the Tabbed style

See the detailed changes applied to this start documented in `SyncOddly_CodeChangeDiary.txt` and committed piecewise to GitHub.

Unit tests added to be able to run Realm-dependent stuff natively on device or simulator. (TBD - link to an unfinished article by [Andy Dent][ADonM].)

### Login
Login flow drawn from [Mark Allibone's article][MA1] and [GitHub sample][MA2].

## Future Plans
Clone to show different architectures, to make comparison easier (it may take a while to have time beyond just the first app!).

## Uncertainties and Open Issues
Note that these will often be discussed in more detail in the `SyncOddly_DesignDecisionsDiary.txt`.

### ViewModel uncertainties
Should we use a VM for even trivial screens? Is the `BaseViewModel` as a `INotifyPropertyChanged` a suitable model for Realm-oriented VMs? (Decided yes, influeced)

I'm not happy about the `IsBusy` approach for list VMs - feels like should abstract.

### Edit Binding
As thrashed out in `SyncOddly_DesignDecisionsDiary.txt` 2023-03-09 we will use the view model to manage this.

This strategy is endorsed by the recent official [MAUI sample][NewTut], editing binds to local fields in the ViewModel, with copying to the Realm object on explicit Save. I'm looking forward to comments on the sample.

### Editing many embedded objects
Discussed in `SyncOddly_DesignDecisionsDiary.txt` 2023-04-15. In particular consider the UX pattern of having nested edits able to be cancelled in one go, vs updated live.

## Testing
Testing is mainly via unit tests to ensure data consistency with operations driven by viewmodels or the main `Doc`.

### Random Data Generator
Using [Faker.Data][FD] to generate data for `SampleDataGenerator` rather than bundling much more logic. This is used in the unit tests as well as in the Settings screen to generate sample data.


### Unit Test Projects
There are two simple test projects created with runners downloaded from [NUnit][NUnitR]. These automatically run all the tests and let you see those that failed.

Note that a weird side-effect occurs in Visual Studio, after running one of these, the list of build configurations adds two ending with _Unit Tests_ which appears to be the IDE sensing these contain tests. However, as they are only to be run on a deployed device (including simulators), the inbuilt test runner just hangs.

Ideally in future there will be a way to run tests from the IDE but it's typical of Realm dotnet to require native instances for testing as it links a C++ core that's only available in those builds.


## Icons
Icons from the [Icons8][i8] collections used under personal license, **not** to be reused.

### Tab Bar - from _Simple Small_ collection
- icons8-today => icon_appts
- icons8-page => icon_notes
- icons8-share => icon_shared
- icons8-settings => icon_settings
- icons8-user => icon_person
- icons8-party => icon_people


#### Configs after running a TestRunner
```
SyncOddly.Android
SyncOddly.iOS
SyncOddlyTestRunner.Android
SyncOddlyTestRunner.Android - Unit Tests
SyncOddlyTestRunner.iOS
SyncOddlyTestRunner.iOS - Unit Tests
```


[RTUT]: https://github.com/mongodb-university/realm-tutorial-dotnet
[NewTut]: https://www.mongodb.com/docs/atlas/app-services/tutorial/dotnet/#overview
[QSdoc]: https://www.mongodb.com/docs/realm/sdk/dotnet/quick-start-with-sync/
[schema]: https://www.mongodb.com/docs/realm/sdk/dotnet/advanced-guides/manual-schema/
[ADonM]: https://andydentperth.medium.com/
[FD]: https://github.com/FermJacob/Faker.Data
[Comments]: https://github.com/AndyDentFree/xamarealms/issues/2
[MVT]: https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/
[NUnitR]: https://docs.nunit.org/articles/xamarin-runners/index.html
[Ma1]: https://mallibone.com/post/xamarin-forms-shell-login
[MA2]: https://github.com/mallibone/ShellLoginSample
[i8]: https://icons8.com/