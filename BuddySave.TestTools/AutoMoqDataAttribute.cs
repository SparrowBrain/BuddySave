using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;

namespace BuddySave.TestTools;

public class AutoMoqDataAttribute() : AutoDataAttribute(() => new Fixture()
    .Customize(new AutoMoqCustomization()));