## Store API

### Repositories
- / [GET] (List Repositories)
- /:repositoryId [GET]
- /:repositoryId [DELETE]

### Objects
- /:repositoryId/objects [GET] (List Objects)
- /:repositoryId/objects/:objectId [GET]
- /:repositoryId/objects/:objectId [DELETE]

### Transfer
- /:repositoryId/transfer/:objectId [GET] (Download)
- /:repositoryId/transfer/:objectId [PUT] (Upload)
- /:repositoryId/transfer/:objectId [POST] (Finish Upload)