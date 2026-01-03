
## 任务：实现 XxController
### 总体要求
1. 使用 MVC 风格的 Controller 实现 API
2. 
### 实现 ArticleController
1. 可以用keyset的方式获取文章(使用 ArticleRepository 的 ListAllArticleAsync 方法)， 路径为 /articles?start=a&limit=b
2. 可以根据给定 id 获取文章详情（使用 ArticleRepository 的 FindByIdAsync 方法）路径为 /article/{id}
3. 可以创建文章（使用 ArticleRepository 的 SaveAsync 方法）路径为 /article，使用 POST 方法
4. 可以更新文章（使用 ArticleRepository 的 SaveAsync 方法）路径为 /article/{id}，使用 PUT 方法 
5. 可以改变文章的状态（使用 ArticleRepository 的 PublishAsync, HiddenAsync 方法）路径为 /article/{id}?status={status}，使用 PATCH 方法

### 实现 TagController
1. 可以获取用keyset 的方式获取所有 Tag （使用 TagRepository 的 ListAllTagAsync 方法）路径为 /tags?start=a&limit=b
2. 可以创建 Tag （使用 TagRepository 的 SaveAsync 方法）路径为 /tag ，使用 POST 方法
3. 可以更新 Tag （使用 TagRepository 的 SaveAsync 方法）路径为 /tag/{id}，使用 PUT 方法
4. 可以用 keyset 的方式获取所有有给定 Tag 的文章 （使用 ArticleRepository 的 ListAllArticleByTagAsync 方法）路径为/tag/articles?start=a&limit=b

### 实现 UserController
1. 可以根据给定 id 获取用户详情（使用 UserRepository 的 FindByIdAsync 方法）路径为 /user/{id}
2. 可以创建用户（使用 UserRepository 的 SaveAsync 方法）路径为 /user/{id}， 使用 POST 方法
3. 可以更新用户（使用 UserRepository 的 SaveAsync 方法）路径为 /user/{id}，使用 PUT 方法
4. 可以获取用户所有文章（使用 ArticleRepository 的 ListByAuthorIdAsync 方法）路径为 /user/{id}/articles?start=a&limit=b

## 实现 UserCredentialController
1. 可以根据给定 id 获取用户凭据详情（使用 UserCredentialRepository 的 FindByIdAsync 方法）路径为 /usercredential/{id}
2. 可以创建用户凭据（使用 UserCredentialRepository 的 SaveAsync 方法）路径为 /usercredential/{id}， 使用 POST 方法
3. 可以更新用户凭据（使用 UserCredentialRepository 的 SaveAsync 方法）路径为 /usercredential/{id}，使用 PUT 方法

## 实现登录
1. 实现用户名密码校验
2. 实现 JWT Token 签发
3. 实现修改登录密码，并且把所有会话踢下线（通过比较jwt里的签发时间和 UserCredential 里的 LastChangedAt 来实现）
4. 实现刷新 Token 的接口

### 要求
1. 使用英语完成文档注释，以及 openapi 注释
2. 添加测试到 Blog.Tests/ControllerTests 下，增量添加，绝对绝对不要破坏项目结构，不要破坏文件结构

### TODO:
1. 使用鉴权保护API