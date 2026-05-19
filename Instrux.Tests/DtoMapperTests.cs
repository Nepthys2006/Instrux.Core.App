using Instrux.Domain.Enums;
using Instrux.Domain.Models;
using Instrux.Services.DTOs;
using Instrux.Services.Mapping;

namespace Instrux.Tests;

public sealed class DtoMapperTests
{
    [Fact]
    public void Teacher_ToDto_MapsCorrectly_ExcludesPassword()
    {
        var teacher = new Teacher
        {
            Id = 1,
            FullName = "John Doe",
            Nickname = "John",
            Email = "john@test.com",
            PasswordHash = "secret"
        };

        var dto = DtoMapper.ToDto(teacher);

        Assert.Equal(1, dto.Id);
        Assert.Equal("John Doe", dto.FullName);
        Assert.Equal("John", dto.Nickname);
        Assert.Equal("john@test.com", dto.Email);
    }

    [Fact]
    public void Assessment_ToDto_And_ToEntity_RoundTrip()
    {
        var dto = new AssessmentDto(0, 1, "Quiz 1", AssessmentType.Quiz, 50m, 0.40m, new DateTime(2025, 1, 15));

        var entity = DtoMapper.ToEntity(dto);
        var result = DtoMapper.ToDto(entity);

        Assert.Equal(dto.ClassId, result.ClassId);
        Assert.Equal(dto.Name, result.Name);
        Assert.Equal(dto.Type, result.Type);
        Assert.Equal(dto.MaxScore, result.MaxScore);
        Assert.Equal(dto.Weight, result.Weight);
        Assert.Equal(dto.Date, result.Date);
    }

    [Fact]
    public void Student_ToDto_And_ToEntity_RoundTrip()
    {
        var dto = new CreateStudentDto("Alice", "STU-100", "alice@test.com", 1);

        var entity = DtoMapper.ToEntity(dto);
        var result = DtoMapper.ToDto(entity);

        Assert.Equal(dto.FullName, result.FullName);
        Assert.Equal(dto.StudentId, result.StudentId);
        Assert.Equal(dto.Email, result.Email);
        Assert.Equal(dto.ClassId, result.ClassId);
    }

    [Fact]
    public void Class_ToDto_IncludesStudentCount()
    {
        var classItem = new Class
        {
            Id = 5,
            Name = "Grade 8 Math",
            Section = "Section B",
            Subject = Subject.Mathematics,
            TeacherId = 1
        };

        var dto = DtoMapper.ToDto(classItem, 10);

        Assert.Equal(5, dto.Id);
        Assert.Equal("Grade 8 Math", dto.Name);
        Assert.Equal("Section B", dto.Section);
        Assert.Equal(Subject.Mathematics, dto.Subject);
        Assert.Equal(1, dto.TeacherId);
        Assert.Equal(10, dto.StudentCount);
    }

    [Fact]
    public void Score_ToDto_MapsCorrectly()
    {
        var score = new Score { Id = 3, StudentId = 1, AssessmentId = 2, Value = 45 };

        var dto = DtoMapper.ToDto(score);

        Assert.Equal(3, dto.Id);
        Assert.Equal(1, dto.StudentId);
        Assert.Equal(2, dto.AssessmentId);
        Assert.Equal(45, dto.Value);
    }

    [Fact]
    public void Score_ToDto_NullValue_MapsCorrectly()
    {
        var score = new Score { Id = 4, StudentId = 1, AssessmentId = 2, Value = null };

        var dto = DtoMapper.ToDto(score);

        Assert.Null(dto.Value);
    }
}
