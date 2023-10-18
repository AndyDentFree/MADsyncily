# Partion Key strategies - (Abandoned)
This was put on hold before I wrote much code, will possibly return to it to explore the contrast between FA and using Partition Keys (PK). **Warning:** _Not all of the implications may have been thought through._

It also seems likely that MongoDB are retiring the entire PartitionKeys approach other than offering legacy support.

If you're not familiar with sync terminology: "Partition" = "server-side Realm", which implies there's a local (synchronised) Realm open for the app.

One problem with this approach is that there's a variable limit of 10 Realms that can be open at once (anecdotally, may be 20 or more depending on hardware but it's limited by _file handles_).

The base idea was to have a three-Realm structure, each of which would be sync with PK:

- **UserMain** - many Realms
  - contain all of a user's data including stuff which may be shared. 
  - This is synchronised only to provide use from multiple devices. It could be a local Realm.
- **SharedUsers** - one Realm providing common way to discover users
  - provides enough bio data to look someone up
  - could be extended to contain a _request to share_ which would notify Ana that Badri wanted her to share something.
- **SharedItems** - many Realms
  - one partition/Realm per sharing relationship. If Ana is sharing some stuff with Badri and different items with Cho, that's two SharedItem realms open.

Informed by [forum discussion of partition keys][PartKeys].

See also the [Partitioning Strategy doc][PartStrat].

### Mixed PK and FS architecture ideas
My original FS architecture idea was a mix, which won't be supported.

- **UserMain** - many Realms, one per user account
  - contain all of a user's data including stuff which may be shared. 
  - This would be synchronised only to allow a given user to have multiple devices. It could be a local Realm but this provides a backup.
  - Each 
   setup as a PartitionKey Realm
- **SharedItems** - a single FlexibleSync realm providing
  - User lookup - basic details of all users are synchronised (this could be migrated to server-side functions for scale)
  - Shared Appointments and Notes

[PartStrat]: https://www.mongodb.com/developer/how-to/realm-partitioning-strategies/
[PartKeys]: https://developer.mongodb.com/community/forums/t/understanding-partition-keys/8317
