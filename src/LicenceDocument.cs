namespace InnovaTest.LicenceGenerator;

// Only the signed token is authoritative. Never trust an unsigned summary.
public sealed record LicenceDocument(string Token);
