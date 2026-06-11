using System.Text.RegularExpressions;
using Application;

namespace Tests.Application;

[TestFixture]
[TestOf(typeof(Constants))]
public class ConstantsTest
{

    [TestCase("2024-07-04-Merijn", "2024", "07", "04", "Merijn")]
    [TestCase("2026-01-15-Antwerpen", "2026", "01", "15", "Antwerpen")]
    [TestCase("2026-06-01-Ardennen", "2026", "06", "01", "Ardennen")]
    public void ProjectNameRegex_MatchesValidProjectFolder(
        string folderName,
        string expectedYear,
        string expectedMonth,
        string expectedDay,
        string expectedProjectName)
    {
        Match match = Constants.ProjectNameRegex.Match(folderName);

        Assert.Multiple(() =>
        {
            Assert.That(match.Success, Is.True);
            Assert.That(match.Groups[1].Value, Is.EqualTo(expectedYear));
            Assert.That(match.Groups[2].Value, Is.EqualTo(expectedMonth));
            Assert.That(match.Groups[3].Value, Is.EqualTo(expectedDay));
            Assert.That(match.Groups[4].Value, Is.EqualTo(expectedProjectName));
        });
    }

    [Test]
    public void ProjectNameRegex_DoesNotMatchInvalidProjectFolder()
    {
        Match match = Constants.ProjectNameRegex.Match("invalid-folder-name");

        Assert.That(match.Success, Is.False);
    }
}