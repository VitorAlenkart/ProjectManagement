using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ProjectManagementAPI.IntegrationsTests.TestSupport;

namespace ProjectManagementAPI.IntegrationsTests;

public sealed class AuthenticationIntegrationTests : IClassFixture<ProjectManagementApiFactory>
{
    private readonly HttpClient _client;
    private readonly ProjectManagementApiFactory _factory;

    public AuthenticationIntegrationTests(ProjectManagementApiFactory factory)
    {
        _factory = factory;
        _factory.ResetDatabase();
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Signup_WhenStudentPayloadIsValid_ReturnsOk()
    {
        var response = await _client.PostAsJsonAsync("/api/signup", new
        {
            email = "new.student@example.com",
            fullName = "New Student",
            password = "Senha@123",
            educationalInstitution = "Test University"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var loginResponse = await _client.PostAsJsonAsync("/api/login", new
        {
            email = "new.student@example.com",
            password = "Senha@123"
        });

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
    }

    [Fact]
    public async Task Signup_WhenTeacherPayloadIsValid_ReturnsOk()
    {
        var response = await _client.PostAsJsonAsync("/api/signup", new
        {
            email = "new.teacher@example.com",
            fullName = "New Teacher",
            password = "Senha@123",
            occupationArea = "Computer Science",
            formationArea = "Software Engineering"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var loginResponse = await _client.PostAsJsonAsync("/api/login", new
        {
            email = "new.teacher@example.com",
            password = "Senha@123"
        });

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
    }

    [Fact]
    public async Task Signup_WhenTeacherPayloadIsInvalid_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/signup", new
        {
            email = "new.teacher@example.com",
            fullName = "New Teacher",
            password = "Senha@123",
            formationArea = "Software Engineering"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Signup_WhenStudentPayloadIsInvalid_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/signup", new
        {
            email = "new.student@example.com",
            fullName = "New Student",
            password = "Senha@123"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Signup_WhenStudentPayloadIsInvalid2_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/signup", new
        {
            email = "",
            fullName = "",
            password = "",
            educationalInstitution = ""
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Signup_WhenEmailAlreadyExists_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/signup", new
        {
            email = "student@example.com",
            fullName = "Duplicate Student",
            password = "Senha@123",
            educationalInstitution = "Test University"
        });

        string body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("Email already in use.", body);
    }

    [Fact]
    public async Task Login_WhenCredentialsAreValid_ReturnsToken()
    {
        var response = await _client.PostAsJsonAsync("/api/login", new
        {
            email = "teacher@example.com",
            password = "Senha@123"
        });

        JsonElement body = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.False(string.IsNullOrWhiteSpace(body.GetProperty("token").GetString()));
    }

    [Fact]
    public async Task Login_WhenPasswordIsInvalid_ReturnsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync("/api/login", new
        {
            email = "teacher@example.com",
            password = "wrong-password"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
