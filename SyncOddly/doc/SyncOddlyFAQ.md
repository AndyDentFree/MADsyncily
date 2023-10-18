# SyncOddly FAQ for Developers

## Q Where's the architecture documented?
Overall arch and the ideas behind it are in `Architecture.md` with some intro in the main `README.md`. 

The decisions leading to that are often in the `../SyncOddly_DesignDecisionsDiary.txt`.

## Q How are updates to Notes or Appointments EmbeddedObjects reconciled?
When we go into an EO edit screen, the original `_Id` is available with that object, along with fields to update.

## Sharing

### Q How do we pass in the items to be shared?
They are shared by Id

## Dependencies
### Q Why did you pick Faker.Data?
I tried [Faker.Net][FN] but it wouldn't link with .Net Standard 2.1 `Failed to resolve assembly: 'Faker.Net.Standard.2.0`. [Faker.Data][FD] was a near drop-in replacement that worked fine.


[FN]: https://www.nuget.org/packages/Faker.Net
[FD]: https://github.com/FermJacob/Faker.Data