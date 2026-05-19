using Instrux.Domain.Models;
using Instrux.Services.DTOs;

namespace Instrux.Application.Services;

public sealed class SessionService
{
    public Teacher CurrentTeacher { get; private set; } = new();
    public bool IsAuthenticated { get; private set; }

    public void SignIn(TeacherDto teacher)
    {
        CurrentTeacher = new Teacher
        {
            Id = teacher.Id,
            FullName = teacher.FullName,
            Nickname = teacher.Nickname,
            Email = teacher.Email
        };
        IsAuthenticated = true;
    }

    public void SignOut()
    {
        CurrentTeacher = new Teacher();
        IsAuthenticated = false;
    }

    public void UpdateCurrentTeacher(TeacherDto teacher)
    {
        CurrentTeacher = new Teacher
        {
            Id = teacher.Id,
            FullName = teacher.FullName,
            Nickname = teacher.Nickname,
            Email = teacher.Email
        };
    }
}
