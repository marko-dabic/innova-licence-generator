namespace InnovaTest.LicenceGenerator.Models;

// Only the signed token is authoritative. Never trust an unsigned summary.
public sealed record LicenceDocument(string Token);
