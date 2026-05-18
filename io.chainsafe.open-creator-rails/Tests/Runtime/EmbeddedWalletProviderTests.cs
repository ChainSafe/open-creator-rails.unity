using System.Numerics;
using System.Threading.Tasks;
using Io.ChainSafe.OpenCreatorRails;
using Nethereum.ABI.EIP712;
using Nethereum.ABI.EIP712.EIP2612;
using Nethereum.Signer;
using Nethereum.Signer.EIP712;
using Nethereum.Util;
using NUnit.Framework;

namespace Tests.Runtime
{
    public class EmbeddedWalletProviderTests : TestsBase
    {
        private IWalletProvider WalletProvider => OpenCreatorRailsService.Instance.WalletProvider;

        [SetUp]
        public async Task SetUp()
        {
            // Always start each test from a known state: account index 0.
            await OpenCreatorRailsService.Instance.Connect(0);
        }

        // -------------------------------------------------------------------------
        // Group 1 — Properties after Connect(0)
        // -------------------------------------------------------------------------

        [Test]
        public void Test_ChainId()
        {
            // Demo scene configures EmbeddedWalletProvider with _chainId = 31337 (Anvil default).
            Assert.AreEqual(new BigInteger(31337), WalletProvider.ChainId);
        }

        [Test]
        public void Test_ConnectedAccountIndex_Default()
        {
            Assert.AreEqual(0, WalletProvider.ConnectedAccountIndex);
        }

        [Test]
        public void Test_ConnectedAccount_IsValidAddress()
        {
            Assert.IsTrue(WalletProvider.ConnectedAccount.Value.IsValidEthereumAddressHexFormat());
        }

        [Test]
        public void Test_ConnectedAccount_MatchesKnownAnvilAddress()
        {
            // HD index 0 of the standard Anvil test mnemonic.
            Assert.AreEqual(
                "0xf39Fd6e51aad88F6F4ce6aB8827279cffFb92266",
                WalletProvider.ConnectedAccount.Value);
        }

        // -------------------------------------------------------------------------
        // Group 2 — Connect() return value & re-connect
        // -------------------------------------------------------------------------

        [Test]
        public async Task Test_Connect_ReturnsWeb3()
        {
            var web3 = await WalletProvider.Connect(0);

            Assert.IsNotNull(web3);
        }

        [Test]
        public async Task Test_Connect_Web3AccountMatchesConnectedAccount()
        {
            var web3 = await WalletProvider.Connect(0);

            // Web3 is constructed with the derived Account; TransactionManager exposes it directly.
            string web3Address = web3.TransactionManager.Account.Address;

            Assert.AreEqual(
                WalletProvider.ConnectedAccount.Value.ToLower(),
                web3Address.ToLower());
        }

        [Test]
        public async Task Test_Connect_IndexOne_UpdatesConnectedAccountIndex()
        {
            await WalletProvider.Connect(1);

            Assert.AreEqual(1, WalletProvider.ConnectedAccountIndex);
        }

        [Test]
        public async Task Test_Connect_IndexOne_MatchesKnownAnvilAddress()
        {
            await WalletProvider.Connect(1);

            // HD index 1 of the standard Anvil test mnemonic.
            Assert.AreEqual(
                "0x70997970C51812dc3A010C7d01b50e0d17dc79C8",
                WalletProvider.ConnectedAccount.Value);
        }

        // -------------------------------------------------------------------------
        // Group 3 — SignMessage
        // -------------------------------------------------------------------------

        [Test]
        public void Test_SignMessage_ReturnsSignature()
        {
            byte[] message = System.Text.Encoding.UTF8.GetBytes("hello open creator rails");

            EthECDSASignature sig = WalletProvider.SignMessage(message);

            Assert.IsNotNull(sig);
            Assert.AreEqual(32, sig.R.Length);
            Assert.AreEqual(32, sig.S.Length);
            Assert.GreaterOrEqual(sig.V.Length, 1);
        }

        [Test]
        public void Test_SignMessage_RecoveredAddressMatchesConnectedAccount()
        {
            byte[] message = System.Text.Encoding.UTF8.GetBytes("hello open creator rails");

            EthECDSASignature sig = WalletProvider.SignMessage(message);

            string hexSig = EthECDSASignature.CreateStringSignature(sig);

            // EthereumMessageSigner.EcRecover applies the EIP-191 prefix internally,
            // matching the prefix applied by EmbeddedWalletProvider.SignMessage.
            string recovered = new EthereumMessageSigner().EcRecover(message, hexSig);

            Assert.AreEqual(
                WalletProvider.ConnectedAccount.Value.ToLower(),
                recovered.ToLower());
        }

        [Test]
        public async Task Test_SignMessage_DifferentAccountsProduceDifferentSignatures()
        {
            byte[] message = System.Text.Encoding.UTF8.GetBytes("hello open creator rails");

            await WalletProvider.Connect(0);
            string sigIndex0 = EthECDSASignature.CreateStringSignature(WalletProvider.SignMessage(message));

            await WalletProvider.Connect(1);
            string sigIndex1 = EthECDSASignature.CreateStringSignature(WalletProvider.SignMessage(message));

            Assert.AreNotEqual(sigIndex0, sigIndex1);
        }

        // -------------------------------------------------------------------------
        // Group 4 — SignTypedData
        // -------------------------------------------------------------------------

        /// <summary>
        /// Builds a minimal but structurally valid EIP-712 typed data definition
        /// using the same factory that Asset.cs uses (EIP-2612 Permit).
        /// The domain and permit values are synthetic test values — no on-chain
        /// call is needed for the signing/recovery round-trip.
        /// </summary>
        private (Permit permit, TypedData<Domain> typedData) BuildTestTypedData()
        {
            var typedData = EIP2612TypeFactory.GetTypedDefinition();
            typedData.Domain = new Domain
            {
                Name             = "TestToken",
                Version          = "1",
                ChainId          = WalletProvider.ChainId,
                VerifyingContract = "0x0000000000000000000000000000000000000001",
            };

            var permit = new Permit
            {
                Owner    = WalletProvider.ConnectedAccount.Value,
                Spender  = "0x0000000000000000000000000000000000000002",
                Value    = new BigInteger(1000),
                Nonce    = BigInteger.Zero,
                Deadline = new BigInteger(9999999999),
            };

            return (permit, typedData);
        }

        [Test]
        public void Test_SignTypedData_ReturnsSignature()
        {
            var (permit, typedData) = BuildTestTypedData();

            EthECDSASignature sig = WalletProvider.SignTypedData(permit, typedData);

            Assert.IsNotNull(sig);
            Assert.AreEqual(32, sig.R.Length);
            Assert.AreEqual(32, sig.S.Length);
            Assert.GreaterOrEqual(sig.V.Length, 1);
        }

        [Test]
        public void Test_SignTypedData_RecoveredAddressMatchesConnectedAccount()
        {
            var (permit, typedData) = BuildTestTypedData();

            EthECDSASignature sig = WalletProvider.SignTypedData(permit, typedData);

            string hexSig = EthECDSASignature.CreateStringSignature(sig);

            // RecoverFromSignatureV4 mirrors the SignTypedDataV4 encoding path used in
            // EmbeddedWalletProvider.SignTypedData.
            string recovered = new Eip712TypedDataSigner().RecoverFromSignatureV4(permit, typedData, hexSig);

            Assert.AreEqual(
                WalletProvider.ConnectedAccount.Value.ToLower(),
                recovered.ToLower());
        }

        // -------------------------------------------------------------------------
        // Group 5 — Disconnect
        // -------------------------------------------------------------------------

        [Test]
        public void Test_Disconnect_CompletesWithoutException()
        {
            Assert.DoesNotThrowAsync(async () => await WalletProvider.Disconnect());
        }
    }
}
