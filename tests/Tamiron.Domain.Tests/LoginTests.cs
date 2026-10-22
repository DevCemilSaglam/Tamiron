using Tamiron.Domain;

namespace Tamiron.Domain.Tests;

/// <summary>
/// K-074 · SMS kodu ve oturum. Kod: 6 hane, 5 dakika, 5 yanlış denemede ölür, bir kez
/// kullanılır; bir telefona 10 dakikada en çok 3 kod. Oturum: süresiz, yalnız çıkışla biter.
/// </summary>
public class LoginTests
{
    static readonly DateTime Now = new(2026, 10, 22, 9, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Issued_code_is_six_digits_and_only_its_hash_is_stored()
    {
        var (lc, code) = LoginCode.Issue("0532 215 30 00", Now);
        Assert.Matches("^[0-9]{6}$", code);
        Assert.DoesNotContain(code, lc.CodeHash);
        Assert.Equal(Now.AddMinutes(5), lc.ExpiresAt);
    }

    [Fact]
    public void Codes_are_not_predictable()
        => Assert.True(Enumerable.Range(0, 20).Select(_ => LoginCode.Issue("0532 215 30 00", Now).Code).Distinct().Count() > 15);

    [Fact]
    public void Right_code_works_once()
    {
        var (lc, code) = LoginCode.Issue("0532 215 30 00", Now);
        lc.Verify(code, Now.AddMinutes(1));
        Assert.Equal("auth.code_used", Assert.Throws<DomainException>(() => lc.Verify(code, Now.AddMinutes(2))).Code);
    }

    [Fact]
    public void Expired_code_fails()
    {
        var (lc, code) = LoginCode.Issue("0532 215 30 00", Now);
        Assert.Equal("auth.code_expired", Assert.Throws<DomainException>(() => lc.Verify(code, Now.AddMinutes(5).AddSeconds(1))).Code);
    }

    [Fact]
    public void Five_wrong_attempts_kill_the_code_even_for_the_right_one()
    {
        var (lc, code) = LoginCode.Issue("0532 215 30 00", Now);
        var wrong = code == "000000" ? "111111" : "000000";
        for (var i = 0; i < 5; i++)
            Assert.Equal("auth.code_wrong", Assert.Throws<DomainException>(() => lc.Verify(wrong, Now)).Code);
        Assert.Equal("auth.code_locked", Assert.Throws<DomainException>(() => lc.Verify(code, Now)).Code);
    }

    [Fact]
    public void At_most_three_codes_per_ten_minutes()
    {
        Assert.True(LoginCode.CanIssue([Now.AddMinutes(-9), Now.AddMinutes(-5)], Now));
        Assert.False(LoginCode.CanIssue([Now.AddMinutes(-9), Now.AddMinutes(-5), Now.AddMinutes(-1)], Now));
        Assert.True(LoginCode.CanIssue([Now.AddMinutes(-11), Now.AddMinutes(-5), Now.AddMinutes(-1)], Now));   // eskisi pencereden çıktı
    }

    [Fact]
    public void Session_token_is_random_hash_matches_and_only_logout_ends_it()
    {
        var (s, token) = Session.Open(Guid.NewGuid(), Guid.NewGuid(), "Ahmet'in telefonu", Now);
        Assert.True(token.Length >= 40);
        Assert.Equal(Session.HashToken(token), s.TokenHash);
        Assert.NotEqual(token, s.TokenHash);
        Assert.True(s.IsActive);                       // süre YOK: bir yıl sonra da açık
        s.Revoke(Now.AddYears(1));
        Assert.False(s.IsActive);
        Assert.NotEqual(Session.Open(Guid.NewGuid(), Guid.NewGuid(), null, Now).Token, token);
    }
}
