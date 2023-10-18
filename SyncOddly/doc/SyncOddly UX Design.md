# SyncOddly UX Design
This page goes through the UX design to fulfill the testing needs of SyncOddly as a demo app, as specified in the `../readme.md` and `./architecture.md`.


## Roles
Because this is a demonstrator project, it has modes built in which would not be in a production app.

### Local vs Sync Data
This is a cross-cutting concern - you can choose to be using a local Realm or synchronised with the server. This allows use of the data generator to explore how the UI works with much larger volumes than you want to sync. It also mirrors the natural flow of an app user starting in a free mode, with local data only, who then pays for a sync subscription. This implies a copying operation to copy all data to the initial Sync realm.

### Debug/DataView mode
In order to see the effects of actions, generating data and then your edits, it's necessary to see raw data:
- lists of the `SharedWith` objects
- ID values so you can see exactly what's shared, especially if comparing edit and update processes when you have otherwise-identical pieces of data

### App mode
This is like an end-user app, which will obscure much of the data model and only expose a limited amount of user data, matching the current selected user.

### UI Requirements
- User management
	- Debug/App Mode selection - bury in Settings screen so top-level App mode looks _normal_
	- Logout
	- Login screen - to use in synch mode or pick local database
	- User selector - for local data, to change the current user for App Mode
- Tab menu for Debug mode
	- User - full details list goes down into
		- EditPerson - personal details with nested lists of Appointments & Notes
		- EditAppointment details
		- EditNote details
	- People - directory of limited details
	- SharedWith
	- Settings
- Tab menu for App Mode
	- Me - straight into single user with nested details
	- Appointments - from current user nested, goes down to
		- EditAppointment details
	- Notes - from current user nested, goes down to
		-  EditNote details
	- People - shows Lookup details
	- Settings
- Common Settings panel
	- mode selector
	- Current User name - links to EditPerson 
	- sample data generator panel

## Under Consideration
- Search panels on all list views