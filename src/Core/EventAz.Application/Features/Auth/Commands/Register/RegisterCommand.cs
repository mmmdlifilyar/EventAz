using MediatR;

namespace EventAz.Application.Features.Auth.Commands.Register;

public record RegisterCommand(
    string FullName,
    string Email,
    string Password,
    string Role   // "User" və ya "Organizer" — Admin qeydiyyatdan yaranmır, seed edilir
) : IRequest<string>;   // qaytarır: JWT token
