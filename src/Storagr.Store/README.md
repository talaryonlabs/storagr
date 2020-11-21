## Store API

### Repositories
- / [GET] (List Repositories)
- / [POST] (Add Repository)
- /:repositoryId [GET]
- /:repositoryId [DELETE]

### Objects
- /:repositoryId/objects [GET] (List Objects)
- /:repositoryId/objects [POST] (Request)
- /:repositoryId/objects/:objectId [GET]
- /:repositoryId/objects/:objectId [DELETE]

### Transfer
- /:repositoryId/transfer/:objectId [GET]
- /:repositoryId/transfer/:objectId [PUT]