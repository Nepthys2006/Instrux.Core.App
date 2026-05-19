using Instrux.Domain.Enums;
using Instrux.Domain.Models;
using Instrux.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Instrux.Tests;

public static class InMemoryDbContextFactory
{
    public static InstruxDbContext Create()
    {
        var options = new DbContextOptionsBuilder<InstruxDbContext>()
            .UseInMemoryDatabase($"InstruxTestDb-{Guid.NewGuid()}")
            .Options;

        var context = new InstruxDbContext(options);
        context.Database.EnsureCreated();

        if (!context.GradingConfigs.Any())
        {
            context.GradingConfigs.AddRange(
                new GradingConfig { Subject = Subject.Mathematics, Group = SubjectGroup.MathScience, WrittenWorksWeight = 0.40m, PerformanceTasksWeight = 0.40m, QuarterlyAssessmentWeight = 0.20m },
                new GradingConfig { Subject = Subject.Science, Group = SubjectGroup.MathScience, WrittenWorksWeight = 0.40m, PerformanceTasksWeight = 0.40m, QuarterlyAssessmentWeight = 0.20m },
                new GradingConfig { Subject = Subject.English, Group = SubjectGroup.LanguagesSocialSciences, WrittenWorksWeight = 0.30m, PerformanceTasksWeight = 0.50m, QuarterlyAssessmentWeight = 0.20m },
                new GradingConfig { Subject = Subject.Filipino, Group = SubjectGroup.LanguagesSocialSciences, WrittenWorksWeight = 0.30m, PerformanceTasksWeight = 0.50m, QuarterlyAssessmentWeight = 0.20m },
                new GradingConfig { Subject = Subject.AralingPanlipunan, Group = SubjectGroup.LanguagesSocialSciences, WrittenWorksWeight = 0.30m, PerformanceTasksWeight = 0.50m, QuarterlyAssessmentWeight = 0.20m },
                new GradingConfig { Subject = Subject.EdukasyonSaPagpapakatao, Group = SubjectGroup.LanguagesSocialSciences, WrittenWorksWeight = 0.30m, PerformanceTasksWeight = 0.50m, QuarterlyAssessmentWeight = 0.20m },
                new GradingConfig { Subject = Subject.TLE, Group = SubjectGroup.SkillsArts, WrittenWorksWeight = 0.20m, PerformanceTasksWeight = 0.60m, QuarterlyAssessmentWeight = 0.20m },
                new GradingConfig { Subject = Subject.HomeEconomics, Group = SubjectGroup.SkillsArts, WrittenWorksWeight = 0.20m, PerformanceTasksWeight = 0.60m, QuarterlyAssessmentWeight = 0.20m },
                new GradingConfig { Subject = Subject.MAPEH, Group = SubjectGroup.SkillsArts, WrittenWorksWeight = 0.20m, PerformanceTasksWeight = 0.60m, QuarterlyAssessmentWeight = 0.20m }
            );
            context.SaveChanges();
        }

        return context;
    }

    public static Teacher CreateTeacher(InstruxDbContext context, string email = "teacher@test.com")
    {
        var teacher = new Teacher
        {
            FullName = "Test Teacher",
            Nickname = "Tester",
            Email = email,
            PasswordHash = "password123"
        };
        context.Teachers.Add(teacher);
        context.SaveChanges();
        return teacher;
    }

    public static Class CreateClass(InstruxDbContext context, int teacherId, Subject subject = Subject.Mathematics)
    {
        var classItem = new Class
        {
            Name = "Test Class",
            Section = "Section A",
            Subject = subject,
            SchoolYear = "2025-2026",
            Semester = "1st",
            CoverColor = "#2C5EAD",
            TeacherId = teacherId
        };
        context.Classes.Add(classItem);
        context.SaveChanges();
        return classItem;
    }

    public static Student CreateStudent(InstruxDbContext context, int classId, string name = "Test Student")
    {
        var student = new Student
        {
            FullName = name,
            StudentId = "STU-001",
            Email = "student@test.com",
            ClassId = classId
        };
        context.Students.Add(student);
        context.SaveChanges();
        return student;
    }
}
