using AutoFixture.Xunit2;

namespace BuddySave.TestTools;

public class InlineAutoMoqDataAttribute : InlineAutoDataAttribute
{
    public InlineAutoMoqDataAttribute(params object[] objects) : base(new AutoMoqDataAttribute(), objects) { }
}