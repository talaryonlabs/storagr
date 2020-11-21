## API

### User
- /users (GET)
- /users/create (POST)
- /users/u/{userId} (GET)
- /users/u/{userId} (PATCH)
- /users/u/{userId} (DELETE)

### Auth
- /auth (GET)
- /auth (POST)
- /auth/logout (GET)

### Object
- /{repositoryId}/objects (GET)
- /{repositoryId}/objects/o/{objectId} (GET)
- /{repositoryId}/objects/o/{objectId} (DELETE)
- /{repositoryId}/objects/v/{objectId} (POST)

##### Batch
- /{repositoryId}/objects/batch (POST)


### Locking
- /{repositoryId}/locks (GET)
- /{repositoryId}/locks (POST)
- /{repositoryId}/locks/verify (POST)
- /{repositoryId}/locks/{lockId}/unlock (POST)