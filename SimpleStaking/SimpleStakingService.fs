namespace SimpleStaking.Contracts.SimpleStaking

open System
open System.Threading.Tasks
open System.Collections.Generic
open System.Numerics
open Nethereum.Hex.HexTypes
open Nethereum.ABI.FunctionEncoding.Attributes
open Nethereum.Web3
open Nethereum.RPC.Eth.DTOs
open Nethereum.Contracts.CQS
open Nethereum.Contracts.ContractHandlers
open Nethereum.Contracts
open System.Threading
open SimpleStaking.Contracts.SimpleStaking.ContractDefinition


    type SimpleStakingService (web3: Web3, contractAddress: string) =
    
        member val Web3 = web3 with get
        member val ContractHandler = web3.Eth.GetContractHandler(contractAddress) with get
    
        static member DeployContractAndWaitForReceiptAsync(web3: Web3, simpleStakingDeployment: SimpleStakingDeployment, ?cancellationTokenSource : CancellationTokenSource): Task<TransactionReceipt> = 
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            web3.Eth.GetContractDeploymentHandler<SimpleStakingDeployment>().SendRequestAndWaitForReceiptAsync(simpleStakingDeployment, cancellationTokenSourceVal)
        
        static member DeployContractAsync(web3: Web3, simpleStakingDeployment: SimpleStakingDeployment): Task<string> =
            web3.Eth.GetContractDeploymentHandler<SimpleStakingDeployment>().SendRequestAsync(simpleStakingDeployment)
        
        static member DeployContractAndGetServiceAsync(web3: Web3, simpleStakingDeployment: SimpleStakingDeployment, ?cancellationTokenSource : CancellationTokenSource) = async {
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            let! receipt = SimpleStakingService.DeployContractAndWaitForReceiptAsync(web3, simpleStakingDeployment, cancellationTokenSourceVal) |> Async.AwaitTask
            return new SimpleStakingService(web3, receipt.ContractAddress);
            }
    
        member this.StakeRequestAsync(stakeFunction: StakeFunction): Task<string> =
            this.ContractHandler.SendRequestAsync(stakeFunction);
        
        member this.StakeRequestAndWaitForReceiptAsync(stakeFunction: StakeFunction, ?cancellationTokenSource : CancellationTokenSource): Task<TransactionReceipt> =
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            this.ContractHandler.SendRequestAndWaitForReceiptAsync(stakeFunction, cancellationTokenSourceVal);
        
        member this.UnstakeRequestAsync(unstakeFunction: UnstakeFunction): Task<string> =
            this.ContractHandler.SendRequestAsync(unstakeFunction);
        
        member this.UnstakeRequestAndWaitForReceiptAsync(unstakeFunction: UnstakeFunction, ?cancellationTokenSource : CancellationTokenSource): Task<TransactionReceipt> =
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            this.ContractHandler.SendRequestAndWaitForReceiptAsync(unstakeFunction, cancellationTokenSourceVal);
        
        member this.ReleaseDatesQueryAsync(releaseDatesFunction: ReleaseDatesFunction, ?blockParameter: BlockParameter): Task<BigInteger> =
            let blockParameterVal = defaultArg blockParameter null
            this.ContractHandler.QueryAsync<ReleaseDatesFunction, BigInteger>(releaseDatesFunction, blockParameterVal)
            
        member this.StakedFundsQueryAsync(stakedFundsFunction: StakedFundsFunction, ?blockParameter: BlockParameter): Task<BigInteger> =
            let blockParameterVal = defaultArg blockParameter null
            this.ContractHandler.QueryAsync<StakedFundsFunction, BigInteger>(stakedFundsFunction, blockParameterVal)
            
        member this.VaraQueryAsync(varaFunction: VaraFunction, ?blockParameter: BlockParameter): Task<string> =
            let blockParameterVal = defaultArg blockParameter null
            this.ContractHandler.QueryAsync<VaraFunction, string>(varaFunction, blockParameterVal)
            
    

