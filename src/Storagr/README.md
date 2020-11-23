## API

### User
- /users [GET]
- /users [POST] (Create)
- /users/me [GET] (Get self)
- /users/:userId [GET]
- /users/:userId [PATCH]
- /users/:userId [DELETE]

#### Authentication
- /users/authenticate [POST]

### Repositories
- / [GET] (List Repositories)
- /:repositoryId [GET]
- /:repositoryId [DELETE]

##
### Object
- /:repositoryId/objects [GET]
- /:repositoryId/objects/:objectId [GET]
- /:repositoryId/objects/:objectId [DELETE]

#### Batch
- /:repositoryId/objects/batch [POST]

#### Verify
- /:repositoryId/objects/verify [POST]

##
### Locking
- /:repositoryId/locks [GET]
- /:repositoryId/locks [POST]
- /:repositoryId/locks/verify [POST]
- /:repositoryId/locks/:lockId [GET]
- /:repositoryId/locks/:lockId/unlock [POST]