using System;
using NUnit.Framework;

namespace LudusSDK.Tests
{
    public sealed class LudusCollectionContextTests
    {
        [SetUp]
        public void SetUp()
        {
            LudusCollectionContext.Clear();
            LudusParticipantContext.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            LudusCollectionContext.Clear();
            LudusParticipantContext.Clear();
        }

        [Test]
        public void TryConfigure_MantemCredencialTemporariaSomenteNoContexto()
        {
            bool configured = LudusCollectionContext.TryConfigure(
                "collection-001",
                "participant-001",
                "Aluna Fictícia",
                "token-temporario-ficticio",
                DateTime.UtcNow.AddMinutes(10).ToString("O"),
                out string errorMessage
            );

            Assert.That(configured, Is.True, errorMessage);
            Assert.That(LudusCollectionContext.IsConnected, Is.True);
            Assert.That(
                LudusCollectionContext.TryGet(
                    out LudusCollectionConnection connection,
                    out errorMessage
                ),
                Is.True,
                errorMessage
            );
            Assert.That(connection.CollectionId, Is.EqualTo("collection-001"));
            Assert.That(connection.ParticipantRef, Is.EqualTo("participant-001"));
            Assert.That(connection.DisplayName, Is.EqualTo("Aluna Fictícia"));
            Assert.That(connection.Token, Is.EqualTo("token-temporario-ficticio"));
        }

        [Test]
        public void TryConfigure_RecusaCredencialExpirada()
        {
            bool configured = LudusCollectionContext.TryConfigure(
                "collection-001",
                "participant-001",
                "Aluna Fictícia",
                "token-temporario-ficticio",
                DateTime.UtcNow.AddMinutes(-1).ToString("O"),
                out string errorMessage
            );

            Assert.That(configured, Is.False);
            Assert.That(errorMessage, Does.Contain("expirada"));
            Assert.That(LudusCollectionContext.IsConnected, Is.False);
        }

        [Test]
        public void ClearParticipant_LimpaTambemConexaoDaColeta()
        {
            Assert.That(
                LudusCollectionContext.TryConfigure(
                    "collection-001",
                    "participant-001",
                    "Aluna Fictícia",
                    "token-temporario-ficticio",
                    DateTime.UtcNow.AddMinutes(10).ToString("O"),
                    out _
                ),
                Is.True
            );
            Assert.That(
                LudusSdk.TrySetParticipant(
                    LudusSdk.StudentIdPendingManualImport,
                    "Aluna Fictícia",
                    out _
                ),
                Is.True
            );

            LudusSdk.ClearParticipant();

            Assert.That(LudusCollectionContext.IsConnected, Is.False);
            Assert.That(
                LudusParticipantContext.TryResolve(
                    string.Empty,
                    out _,
                    out _
                ),
                Is.False
            );
        }
    }
}
