using Moq;
using Stratis.SmartContracts;
using Stratis.SmartContracts.CLR;
using System;
using System.Reflection;
using Xunit;

namespace PassportApplicationContract.Tests
{
    public class PassportApplicationTests
    {
        private const string AppId = "882B9E48-F3D0-4E3C-92B7-AF1F3851335D";
        private const string Applicant = "0x0000000000000000000000000000000000000001";
        private const string Provider = "0x0000000000000000000000000000000000000002";
        private const string RefNumber = "123456";

        private static readonly Address ApplicantAddress = Applicant.HexToAddress();
        private static readonly Address ProviderAddress = Provider.HexToAddress();
        private readonly Mock<ISmartContractState> mockContractState;
        private readonly Mock<IPersistentState> mockPersistentState;
        private readonly Mock<IInternalTransactionExecutor> mockInternalExecutor;
        private readonly Mock<IContractLogger> mockContractLogger;

        public PassportApplicationTests()
        {
            this.mockContractLogger = new Mock<IContractLogger>();
            this.mockPersistentState = new Mock<IPersistentState>();
            this.mockInternalExecutor = new Mock<IInternalTransactionExecutor>();
            this.mockContractState = new Mock<ISmartContractState>();
            this.mockContractState.Setup(s => s.PersistentState).Returns(this.mockPersistentState.Object);
            this.mockContractState.Setup(s => s.ContractLogger).Returns(this.mockContractLogger.Object);
            this.mockContractState.Setup(s => s.InternalTransactionExecutor).Returns(this.mockInternalExecutor.Object);
        }

        [Theory]
        [InlineData(nameof(PassportApplication.State))]
        [InlineData(nameof(PassportApplication.AppId))]
        [InlineData(nameof(PassportApplication.Provider))]
        [InlineData(nameof(PassportApplication.ReferenceNumber))]
        [InlineData(nameof(PassportApplication.Applicant))]
        public void Property_Setter_Is_Private(string propertyName)
        {
            Type type = typeof(PassportApplication);

            PropertyInfo property = type.GetProperty(propertyName);

            Assert.True(property.SetMethod.IsPrivate);
        }

        [Fact]
        public void Constructor_Sets_Initial_Values()
        {
            this.mockContractState.Setup(s => s.Message.Sender).Returns(ApplicantAddress);

            var passportApplication = new PassportApplication(this.mockContractState.Object, AppId, ProviderAddress, RefNumber);

            this.mockPersistentState.Verify(s => s.SetString(nameof(PassportApplication.AppId), AppId));
            this.mockPersistentState.Verify(s => s.SetAddress(nameof(PassportApplication.Applicant), ApplicantAddress));
            this.mockPersistentState.Verify(s => s.SetAddress(nameof(PassportApplication.Provider), ProviderAddress));
            this.mockPersistentState.Verify(s => s.SetString(nameof(PassportApplication.ReferenceNumber), RefNumber));
            this.mockPersistentState.Verify(s => s.SetUInt32(nameof(PassportApplication.State), (uint)PassportApplication.StateType.MakeAppointment));
        }

        [Fact]
        public void Pay_Fails_Sender_Not_Applicant()
        {
            // Setup incorrect sender.
            this.mockContractState.Setup(s => s.Message.Sender).Returns(Address.Zero);

            var contract = new PassportApplication(this.mockContractState.Object, AppId, ProviderAddress, RefNumber);
            
            // Setup set state.
            this.mockPersistentState.Setup(s => s.GetUInt32(nameof(PassportApplication.State))).Returns((uint)PassportApplication.StateType.MakeAppointment);

            // Attempt pay of wrong applicant should fail.
            Assert.Throws<SmartContractAssertException>(contract.Pay);
        }

        //[Fact]
        //public void TransferResponsibility_Fails_State_Is_Completed()
        //{
        //    var contract = this.NewPassportApplication();

        //    // Setup correct sender.
        //    this.mockContractState.Setup(s => s.Message.Sender).Returns(CounterPartyAddress);

        //    // Setup counterparty address.
        //    this.mockPersistentState.Setup(s => s.GetAddress(nameof(PassportApplication.CounterParty))).Returns(CounterPartyAddress);

        //    // Setup state = completed.
        //    this.mockPersistentState.Setup(s => s.GetUInt32(nameof(PassportApplication.State))).Returns((uint)PassportApplication.StateType.Completed);

        //    // Attempt to transfer to any address should fail.
        //    Assert.Throws<SmartContractAssertException>(() => contract.TransferResponsibility(Address.Zero));
        //}

        //[Fact]
        //public void TransferResponsibility_Succeeds_State_Is_Created()
        //{
        //    var contract = this.NewPassportApplication();

        //    // Setup correct sender.
        //    this.mockContractState.Setup(s => s.Message.Sender).Returns(CounterPartyAddress);

        //    // Setup counterparty address.
        //    this.mockPersistentState.Setup(s => s.GetAddress(nameof(PassportApplication.CounterParty))).Returns(CounterPartyAddress);

        //    // Setup state = created.
        //    this.mockPersistentState.Setup(s => s.GetUInt32(nameof(PassportApplication.State))).Returns((uint)PassportApplication.StateType.Created);

        //    // Attempt to transfer to any address should succeed.
        //    contract.TransferResponsibility(IntermediaryAddress);

        //    this.mockPersistentState.Verify(s => s.SetUInt32(nameof(PassportApplication.State), (uint)PassportApplication.StateType.InTransit), Times.Once);
        //    this.mockPersistentState.Verify(s => s.SetAddress(nameof(PassportApplication.PreviousCounterParty), CounterPartyAddress), Times.Once);
        //    this.mockPersistentState.Verify(s => s.SetAddress(nameof(PassportApplication.CounterParty), IntermediaryAddress), Times.Once);
        //}

        //[Fact]
        //public void TransferResponsibility_Succeeds_State_Is_InTransit()
        //{
        //    var contract = this.NewPassportApplication();

        //    // Setup correct sender.
        //    this.mockContractState.Setup(s => s.Message.Sender).Returns(CounterPartyAddress);

        //    // Setup counterparty address.
        //    this.mockPersistentState.Setup(s => s.GetAddress(nameof(PassportApplication.CounterParty))).Returns(CounterPartyAddress);

        //    // Setup state = in transit.
        //    this.mockPersistentState.Setup(s => s.GetUInt32(nameof(PassportApplication.State))).Returns((uint)PassportApplication.StateType.InTransit);

        //    // Attempt to transfer to any address should succeed.
        //    contract.TransferResponsibility(IntermediaryAddress);

        //    // Make sure the state is not changed.
        //    this.mockPersistentState.Verify(s => s.SetUInt32(nameof(PassportApplication.State), It.IsAny<uint>()), Times.Never);

        //    this.mockPersistentState.Verify(s => s.SetAddress(nameof(PassportApplication.PreviousCounterParty), CounterPartyAddress), Times.Once);
        //    this.mockPersistentState.Verify(s => s.SetAddress(nameof(PassportApplication.CounterParty), IntermediaryAddress), Times.Once);
        //}

        //[Fact]
        //public void Complete_Fails_Sender_Not_SupplyChainOwner()
        //{
        //    var contract = this.NewPassportApplication();

        //    // Setup incorrect sender.
        //    this.mockContractState.Setup(s => s.Message.Sender).Returns(Address.Zero);

        //    // Setup supplychainowner address.
        //    this.mockPersistentState.Setup(s => s.GetAddress(nameof(PassportApplication.SupplyChainOwner))).Returns(SupplyChainOwnerAddress);

        //    // Attempt to call completion with incorrect sender should fail.
        //    Assert.Throws<SmartContractAssertException>(() => contract.Complete());
        //}

        //[Fact]
        //public void Complete_Fails_State_Is_Completed()
        //{
        //    var contract = this.NewPassportApplication();

        //    // Setup correct sender.
        //    this.mockContractState.Setup(s => s.Message.Sender).Returns(SupplyChainOwnerAddress);

        //    // Setup correct supplychainowner address.
        //    this.mockPersistentState.Setup(s => s.GetAddress(nameof(PassportApplication.SupplyChainOwner))).Returns(SupplyChainOwnerAddress);

        //    // Setup state = completed.
        //    this.mockPersistentState.Setup(s => s.GetUInt32(nameof(PassportApplication.State))).Returns((uint)PassportApplication.StateType.Completed);

        //    // Attempt to call completion with incorrect state should fail.
        //    Assert.Throws<SmartContractAssertException>(() => contract.Complete());
        //}

        //[Theory]
        //[InlineData((uint)PassportApplication.StateType.InTransit)]
        //[InlineData((uint)PassportApplication.StateType.Created)]
        //public void Complete_Succeeds(uint state)
        //{
        //    var contract = this.NewPassportApplication();

        //    // Setup correct sender.
        //    this.mockContractState.Setup(s => s.Message.Sender).Returns(SupplyChainOwnerAddress);

        //    // Setup correct supplychainowner address.
        //    this.mockPersistentState.Setup(s => s.GetAddress(nameof(PassportApplication.SupplyChainOwner))).Returns(SupplyChainOwnerAddress);

        //    // Setup correct counterparty address.
        //    this.mockPersistentState.Setup(s => s.GetAddress(nameof(PassportApplication.CounterParty))).Returns(CounterPartyAddress);

        //    // Setup current state.
        //    this.mockPersistentState.Setup(s => s.GetUInt32(nameof(PassportApplication.State))).Returns(state);

        //    // Attempt to call completion with incorrect state should fail.
        //    contract.Complete();

        //    this.mockPersistentState.Verify(s => s.SetUInt32(nameof(PassportApplication.State), (uint)PassportApplication.StateType.Completed));
        //    this.mockPersistentState.Verify(s => s.SetAddress(nameof(PassportApplication.PreviousCounterParty), CounterPartyAddress));
        //    this.mockPersistentState.Verify(s => s.SetAddress(nameof(PassportApplication.CounterParty), Address.Zero));
        //}

        //private PassportApplication NewPassportApplication()
        //{
        //    this.mockContractState.Setup(s => s.Message.Sender).Returns(InitiatingCounterPartyAddress);

        //    var result = new PassportApplication(this.mockContractState.Object, SupplyChainOwnerAddress, SupplyChainObserverAddress);

        //    // Reset the invocations that happened in the constructor so we don't accidentally test them.
        //    this.mockPersistentState.Invocations.Clear();

        //    return result;
        //}
    }
}
