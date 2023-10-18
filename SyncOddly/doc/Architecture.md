# SyncOddly Architecture

## Overview
This architecture is skewed heavily **owner-editing** with **one-way relationships**. That means:

- **owner-editing:** only the person who creates an item can change it
- **one-way relationship:s** in a sharing relationship, any of the parties can share something but each relationship has a single publisher and one or more consumers. There's not one person broadcasting to all the rest. This is **not** like a forum where, when someone posts, hundreds of thousands see.

These restrictions match the most common pattern of _semi-private_ chat systems and feeds - you put your own content out and nobody else can edit it.

They also create some simplification opportunities for both the UI and the data architecture.

Completely open shared systems, like editing a Google Doc, are a lot more complex and flexible than we often need.

## SyncOddly design

SyncOddly _conceptually_ has three Realm classes (this gets more complicated for sync-oriented implementation, as detailed further down):

- **Person** contains basic user details, owning two collections of `EmbeddedObject`s:
- **Appointment** a shareable appointment such as a meeting
- **Note** a shareable simple piece of text (rather than confusing with the Realm sample _Task Tracker_ by having a todo item)

To match real-world apps in this space, consider people who may eventually have hundreds of nested `Appointment` and dozens of `Note` objects.

### Data Editing Operations
In terms of the relative frequency of changing items, in decreasing order:
- creating or editing Appointments and Notes
- sharing an item with 1-N people
- changing a sharing relationship on a single item (from either side)
- changing a sharing relationship on all items shared with a given person
- creating People/signing up

These frequences affect how we decide to implement sharing. Note the important ability to **cancel** a sharing relationship. It's not a one-time broadcast like sending out an email. Sharing an individual item with one or more people can be undone and a recipient can remove the share. The data structures should support both removing an individual item from being shared as well as all items shared with a given recipient.

There are unit tests covering most of the above scenarios.

### Data Operations not implemented
These are more sophisticated features you might consider in a full app but require much more data to be retained:
- restoring a canceled share
- undoing owner delete of an item
- reinstating sharing of an undone delete
- retaining past appointments even if owner later cancels them

### User features nice to have
Beyond the data editing above, things that would be nice to be able see (exercise for later):
- see **who** you share with
- see who shares with **you**
- see when things changed status (that appointment you swore was there but someone cancelled)
- see **what** you shared and when


## Thoughts on architecture
Redundancy vs propagating edits - brainstorming, see the Design Decisions Diary for more detail on pros & cons.

**Bucket** approach is to have a sharing relationship between two people, or treat it as a group, and add nested items to that as owned objects. They will automatically propagate and sync. However, there are now two copies at least. This works best with low rates of _changes_ to shared items.

**Relational** approach is to get a list of shared items via the bucket then retrieve each, which is very heavy loading on the sync and hence users' battery with many network calls. (note that this may be where GraphQL would shine?)

**Inverse bucket** list is that each shareable item has an owned list of who it is shared with and you sync query on items with your ID in that list.


### Realm-first architecture
This is unashamedly opinionated code. I've stripped away many of the abstractions provided in default Xamarin Forms apps so the Realm classes can be used directly. It's intended to exercise the Realm features first and foremost, without always being an example of _traditional_ Xamarin Forms thinking.

I worked on the C# SDK for Realm from [2015-2017][ADF] and in the occasional tech support we devs provided, found people struggled with the desire to use a _Repository pattern_ and hide Realm classes beneath abstractions.

So, this is an alternative view as a sample. The primary aim is to have a robust architecture with Realm sharing sparsely-synchronised data, with a Xamarin Forms UI.

### ViewModels
_Classical_ Xamarin Forms app structures use a `ViewModel` for each BindingSource.

The official MongoDB Realm samples just have an `ObservableCollection`.

This sample uses unique `ViewModel` classes backing everything, which leads to a small amount of redundancy.

A `ViewModel` for a list will typically have a `Realm` property, initialised from a `Func<Realm>` parameter which acts as a factory (it may be wrapping an instance from higher, or getting a new one).

## Alternative Architectures

**Note** I'm using _Ana, Badri & Cho,_ instead of the traditional Western _Alice, Bob & Carol,_ when participant names needed to describe comms and data sync flows.


### <a id="fs-strat" />FlexibleSync strategies

For the first FlexibleSync architecture, a **single Flexible Sync Realm** is used, matching the ideas in how [RChat sample was updated for Flexible Sync][RChatFS].

Using a single FS is necessary because, as of 2022-05-03, [Ian Ward confirmed][NoMix] that you cannot mix a Partition Key approach and Flexible Sync in a single app.


#### Realm Classes in the FlexibleSync architecture

- **Person** details including private ones, one record only accessed by the creator
- **PersonLookup** details sufficiently to look up someone, shared to all (for now)
- **Appointment** and 
- **Note** as `EmbeddedObject`s described above
- **SharedWith** manages shared items, it's an _owner_ of duplicated, shared items.

Very similar patterns apply to `Appointment` and `Note` objects, so they will be described as _EmSOs__ (Embedded Shared Objects).

Each `EmSO`, in addition to being copied into a `SharedWith` owned collection, has an explicit `IsShared` boolean property. This is more redundancy but allows for UI variation and filtering when looking at the basic `Person`.

A nuance to reduce the initial sync overhead would be to have **PersonLookup** not used locally and to rely on server-side functions to lookup people. Issues with this approach:
1. You cannot see details of other users whilst offline 
2. I want to avoid requiring functions for this sample - I've tried to make it something where **all the logic is in the app.**


#### FlexibleSync Permissions

Comparing to the [RChat sample][RChatFS]:

- The shared **Appointment** and **Note** correspond to his **ChatMessage**, being embedded in **Sharedwith** with 
  - a restriction only the owner can write to them: `"authorID": "%%user.id"`
  - only a recipient can read them
- **PersonLookup** corresponds to **Chatster** and can only be written by the owner but downloaded by all
- **Person** can only be read or written by the owner matched on Id

#### Bootstrapping Test data
This code also works as a test data generator to see how sync volumes perform.

Normally you would have a single `Person` object synchronised.

When you create records with the sample generator, it creates a bunch of local `Person` objects.

[ADF]: https://github.com/realm/realm-dotnet/graphs/contributors
[RChatFS]: https://www.mongodb.com/developer/products/realm/realm-flex-sync-tutorial/
[NoMix]: https://www.mongodb.com/community/forums/t/flexible-sync-questions/150871/12?u=andy_dent
