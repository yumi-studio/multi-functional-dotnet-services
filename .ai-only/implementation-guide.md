# Multi-Functional .NET Services - Implementation Guide

**Last Updated:** February 8, 2026  
**Project:** Multi-Functional Microservices Architecture  
**Stack:** ASP.NET Core 9.0, Entity Framework Core

---

## Table of Contents

1. [Project Overview](#project-overview)
2. [Architecture Pattern](#architecture-pattern)
3. [Service Descriptions](#service-descriptions)
4. [Technology Stack](#technology-stack)
5. [Common Structure](#common-structure)
6. [Implementation Guidelines](#implementation-guidelines)
7. [Best Practices](#best-practices)
8. [Data Flow Patterns](#data-flow-patterns)

---

## Project Overview

This is a multi-service microservices solution composed of two independent ASP.NET Core services that can operate independently or in conjunction:

- **UserCenter Service**: User authentication, authorization, and profile management
- **Fakebook Service**: Social media functionality (posts, comments, reactions, profiles)

Both services follow a layered architecture pattern with clear separation of concerns.

---

## Architecture Pattern

### Layered Architecture (N-Tier)

Each service implements the following layers:

```
Controller Layer (HTTP Endpoints)
    ↓
Service Layer (Business Logic)
    ↓
Repository Layer (Data Access)
    ↓
Database Layer (Entity Framework Core)
```

### Key Characteristics

- **Feature-Based Organization**: Code is organized by features/domains
- **Dependency Injection**: Constructor-based DI for loose coupling
- **Entity Framework Core**: ORM for database access with migrations
- **Repository Pattern**: Abstract data access logic

---

## Service Descriptions

### UserCenter Service

**Purpose**: Authentication, authorization, and user account management

**Key Responsibilities**:
- User registration and login
- JWT token generation and validation
- OAuth integration (Google, Microsoft)
- Token blacklist management
- User profile data management

**Core Components**:

```
UserCenter/
├── Controllers/
│   ├── AuthController        → JWT & OAuth authentication endpoints
│   ├── UsersController       → User management endpoints
│   └── GenericController     → Base controller functionality
├── Services/
│   ├── AuthService          → Authentication logic
│   └── UserService          → User operations
├── Repositories/
│   ├── UserRepository       → User data access
│   └── TokenRepository      → Token blacklist management
├── Models/
│   ├── User                 → User entity
│   ├── UserDTO              → Data transfer object
│   ├── UserExternal         → External auth data
│   └── Token                → Token blacklist entry
└── Features/
    └── AuthFeature/         → Login & Register implementations
```

**Authentication Methods**:
- JWT (JSON Web Tokens)
- Google OAuth
- Microsoft OAuth
- Cookie-based authentication

### Fakebook Service

**Purpose**: Social media platform with posts, comments, and interactions

**Key Responsibilities**:
- Post creation, updating, deletion
- Comment management on posts
- User reactions (likes/emojis) on posts and comments
- User profiles and follow functionality
- Media/file uploads
- Profile customization

**Core Components**:

```
Fakebook/
├── Controllers/
│   ├── FakebookController          → Post operations
│   ├── FakebookCommentController   → Comment operations
│   ├── FakebookPostController      → Post lifecycle
│   ├── FakebookProfileController   → Profile management
│   ├── FakebookUploadController    → Media upload
│   └── GenericController           → Base functionality
├── Services/
│   ├── PostService                 → Post business logic
│   ├── CommentService              → Comment logic
│   ├── ProfileService              → Profile logic
│   └── FileUploadService           → Upload handling
├── Repositories/
│   ├── PostRepository              → Post data access
│   ├── PostCommentRepository       → Comment data access
│   ├── ProfileRepository           → Profile data access
│   └── ReactionRepository          → Reaction data access
├── Models/
│   ├── Entities/                   → Domain entities
│   └── DTOs/                       → Data transfer objects
├── Features/
│   ├── PostFeature/                → Post creation/updates
│   ├── CommentFeature/             → Comment operations
│   ├── ProfileFeature/             → Profile management
│   └── UploadFeature/              → File uploads
├── EventHandlers/                   → Domain event handlers
│   ├── PostDeleted/                → Post deletion events
│   └── PostMediaDeleted/           → Media cleanup events
└── Infrastructure/
    └── UploadMethods/              → Abstract upload strategies
```

**Key Enumerations**:
- `MediaType`: Image, Video, etc.
- `PostVisibility`: Public, Private, Friends-only
- `ReactionType`: Like, Love, Haha, Wow, Sad, Angry
- `ReactionTargetType`: Post, Comment
- `UploadMethod`: Local, Cloud storage, etc.

---

## Technology Stack

### Core Framework
- **Framework**: ASP.NET Core 9.0
- **ORM**: Entity Framework Core

### Authentication & Security
- **JWT Tokens**: Bearer token authentication
- **OAuth**: Google & Microsoft integration
- **Cookie Management**: Secure cookie handling
- **Password**: BCrypt or similar hashing

### Database
- **Provider**: SQL Server (based on migrations)
- **Migrations**: EF Core code-first migrations
- **Pattern**: Repository pattern for data access

### Other Libraries
- **Dependency Injection**: Built-in Microsoft.Extensions.DependencyInjection
- **Logging**: Microsoft.Extensions.Logging
- **Configuration**: appsettings.json (Environment-specific JSON configs)

---

## Common Structure

### Configuration Files

**appsettings.json**
```json
{
  "ConnectionStrings": { "DefaultConnection": "..." },
  "Jwt": { "SecretKey": "...", "Issuer": "...", "Audience": "..." },
  "Authentication": { "Google": {...}, "Microsoft": {...} },
  "Storage": { "Provider": "...", "Path": "..." }
}
```

**appsettings.Development.json** - Development-specific overrides with sensitive data

### Program.cs Pattern

Typical initialization pattern:
1. Builder creation and configuration loading
2. Service registration (Services, Repositories, DbContext)
3. Authentication/Authorization setup
4. Middleware configuration
5. Database migration application

### Database Context Pattern

```csharp
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    
    // DbSets for each entity
    public DbSet<User> Users { get; set; }
    public DbSet<Post> Posts { get; set; }
    // ... more entities
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Entity configurations
    }
}
```

### Repository Pattern

Base functionality:
```csharp
public interface IRepository<T> where T : class
{
    Task<T> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    Task SaveChangesAsync();
}
```

Specific repositories extend/implement as needed.

### Service Layer Pattern

```csharp
public interface IUserService
{
    Task<UserDTO> GetUserAsync(int userId);
    Task UpdateUserAsync(int userId, UpdateUserDTO dto);
    // ... business logic contracts
}

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }
    // ... implementations
}
```

### Controller Pattern

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : GenericController
{
    private readonly IUserService _userService;
    
    public UsersController(IUserService userService)
    {
        _userService = userService;
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<UserDTO>> GetUser(int id)
    {
        // Implementation
    }
}
```

---

## Implementation Guidelines

### Adding a New Feature

#### Step 1: Define Entity Models
- Create entity class in `Models/Entities/`
- Add corresponding DbSet to AppDbContext
- Add data annotations for validation and constraints

#### Step 2: Create Database Migration
```bash
dotnet ef migrations add <MigrationName>
dotnet ef database update
```

#### Step 3: Create Data Transfer Objects (DTOs)
- Create DTOs in `Models/DTOs/`
- Use for request/response serialization
- Keep DTOs separate from entities for flexibility

#### Step 4: Implement Repository
- Create interface in `Repositories/Interfaces/`
- Implement in `Repositories/`
- Inherit from `BaseRepository<T>` if possible
- Handle all data access for the entity

#### Step 5: Implement Service
- Create interface in `Services/Interfaces/`
- Implement in `Services/`
- Inject required repositories
- Implement business logic
- Handle validation and error cases

#### Step 6: Create Controller
- Extend `GenericController`
- Inject required services
- Create endpoints following REST conventions
- Use proper HTTP methods and status codes
- Add route attributes

#### Step 7: Add to Program.cs
```csharp
// Add DbContext
services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// Register services
services.AddScoped<IMyService, MyService>();
services.AddScoped<IMyRepository, MyRepository>();
```

### Authentication Implementation

**UserCenter Specific**:
1. User registers with email/password
2. Credentials validated, user created
3. JWT token generated with custom claims
4. Token returned to client
5. OAuth redirects user to provider
6. External identity verified
7. User linked or created
8. Token issued

**Token Blacklist**:
- On logout, add token to blacklist
- Middleware checks blacklist on each request
- Expired tokens removed periodically

### Authorization

**Claims-Based Authorization**:
```csharp
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class ProtectedController : ControllerBase { }

[Authorize(Policy = "AdminOnly")]
public async Task AdminEndpoint() { }
```

**Custom Claims**:
- `UserId`: User identifier
- `Email`: User email
- `Role`: User role (User, Moderator, Admin)

---

## Best Practices

### Code Organization

1. **Namespace Consistency**: Match folder structure with namespace
2. **Single Responsibility**: Each class has one reason to change
3. **DRY Principle**: Reuse code in base classes and helpers
4. **Explicit Dependencies**: Constructor injection, no service locator

### Error Handling

1. **Custom Exceptions**:
   - `UnauthorizedException` → 401 Unauthorized
   - `UserNotExistException` → 404 Not Found
   - Create domain-specific exceptions

2. **Middleware Error Handling**:
   - Catch exceptions in `RequestErrorMiddleware`
   - Return consistent error responses
   - Log errors appropriately

3. **Validation**:
   - Use data annotations on DTOs
   - Validate in service layer before persistence
   - Return meaningful error messages

### Database Design

1. **Migrations**:
   - One migration per feature
   - Descriptive migration names
   - Test migrations before pushing
   - Never share DbContext between services

2. **Entity Relationships**:
   - Use navigation properties for related entities
   - Configure relationships explicitly
   - Handle cascade deletes appropriately

3. **Performance**:
   - Use `.AsNoTracking()` for read-only queries
   - Include related entities explicitly
   - Index frequently queried columns

### API Design

1. **RESTful Conventions**:
   - GET → Retrieve resource
   - POST → Create resource
   - PUT/PATCH → Update resource
   - DELETE → Remove resource

2. **Versioning**: Consider API versioning for future compatibility

3. **Response Format**:
   ```csharp
   // Success
   { "success": true, "data": {...} }
   
   // Error
   { "success": false, "error": "message" }
   ```

### Async/Await

- Use `async/await` throughout the stack
- Database calls are async (EF Core)
- Avoid `.Result` and `.Wait()`
- Return `Task<T>` from service methods

### Logging

1. Inject `ILogger<T>` into classes
2. Log at appropriate levels
3. Include context in log messages
4. Avoid logging sensitive data

### Testing Approach

```csharp
// Unit test service logic
[Test]
public async Task CreateUser_ValidInput_ReturnsUserDTO()
{
    // Arrange
    var mockRepository = new Mock<IUserRepository>();
    var service = new UserService(mockRepository.Object);
    
    // Act
    var result = await service.CreateUserAsync(dto);
    
    // Assert
    Assert.IsNotNull(result);
}
```

---

## Data Flow Patterns

### User Registration (UserCenter)

```
POST /api/auth/register
    ↓
AuthController.Register()
    ↓
AuthService.RegisterAsync()
    ├─ Validate email not exists
    ├─ Hash password
    ├─ Create User entity
    └─ UserRepository.AddAsync()
    ↓
Database (Insert User)
    ↓
Return JWT Token
```

### Create Post (Fakebook)

```
POST /api/posts
    ↓
FakebookPostController.Create()
    ├─ Validate JWT token
    ├─ Extract UserId from claims
    └─ PostService.CreateAsync()
    ↓
PostService
    ├─ Create Post entity
    ├─ PostRepository.AddAsync()
    └─ Trigger OnPostCreated event
    ↓
Database (Insert Post)
    ↓
202 Accepted or 201 Created
```

### Media Upload (Fakebook)

```
POST /api/upload (multipart/form-data)
    ↓
FakebookUploadController.Upload()
    ├─ Validate file
    └─ FileUploadService.UploadAsync()
    ↓
IUploadMethodResolver
    ├─ Resolve upload strategy (Local, Cloud, etc.)
    └─ Execute upload
    ↓
FileSystem or Cloud Storage
    ↓
Return Upload Result with URL
```

### Comment on Post (Fakebook)

```
POST /api/posts/{postId}/comments
    ↓
FakebookCommentController.Create()
    ↓
CommentService.CreateAsync()
    ├─ Validate post exists
    ├─ Create Comment entity
    ├─ PostCommentRepository.AddAsync()
    └─ Trigger OnCommentCreated event
    ↓
Database (Insert Comment)
    ├─ Update Post.CommentCount
    └─ Save changes
    ↓
Return CommentDTO
```

---

## Future Considerations

### Scalability

1. **Separation of Concerns**: Services can run on different servers
2. **Database**: Consider read replicas for read-heavy operations
3. **Caching**: Add Redis for frequent queries (user profiles, posts)
4. **Event Bus**: Implement message queue for inter-service communication

### Security Enhancements

1. Rate limiting on authentication endpoints
2. Two-factor authentication (2FA)
3. Token refresh mechanism
4. API key authentication for service-to-service calls
5. SSL/TLS certificate pinning for mobile clients

### Monitoring & Observability

1. Application Insights integration
2. Structured logging with correlation IDs
3. Performance monitoring
4. Error tracking (Sentry, etc.)
5. Health check endpoints

### Testing

1. Unit tests for services
2. Integration tests for controllers
3. Database tests for repositories
4. End-to-end API tests
5. Performance/load testing

---

## Quick Reference: API Endpoints

### UserCenter APIs

```
POST   /api/auth/register           → Register new user
POST   /api/auth/login              → Login with credentials
POST   /api/auth/google             → Google OAuth callback
POST   /api/auth/microsoft          → Microsoft OAuth callback
POST   /api/auth/logout             → Logout (blacklist token)
GET    /api/auth/verify             → Verify token validity
GET    /api/users/{id}              → Get user profile
PUT    /api/users/{id}              → Update user profile
```

### Fakebook APIs

```
GET    /api/posts                   → List posts (feed)
POST   /api/posts                   → Create post
PUT    /api/posts/{id}              → Update post
DELETE /api/posts/{id}              → Delete post
POST   /api/posts/{id}/react        → React to post
GET    /api/posts/{id}/comments     → List comments
POST   /api/posts/{id}/comments     → Create comment
PUT    /api/comments/{id}           → Update comment
DELETE /api/comments/{id}           → Delete comment
POST   /api/comments/{id}/react     → React to comment
GET    /api/profiles/{userId}       → Get user profile
POST   /api/profiles                → Create/update profile
POST   /api/upload                  → Upload media
```

---

**Document Version**: 1.0  
**Audience**: Development Team, Code Reviewers, New Contributors
