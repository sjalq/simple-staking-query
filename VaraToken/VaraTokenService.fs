namespace SimpleStaking.Contracts.VaraToken

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
open SimpleStaking.Contracts.VaraToken.ContractDefinition


    type VaraTokenService (web3: Web3, contractAddress: string) =
    
        member val Web3 = web3 with get
        member val ContractHandler = web3.Eth.GetContractHandler(contractAddress) with get
    
        static member DeployContractAndWaitForReceiptAsync(web3: Web3, varaTokenDeployment: VaraTokenDeployment, ?cancellationTokenSource : CancellationTokenSource): Task<TransactionReceipt> = 
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            web3.Eth.GetContractDeploymentHandler<VaraTokenDeployment>().SendRequestAndWaitForReceiptAsync(varaTokenDeployment, cancellationTokenSourceVal)
        
        static member DeployContractAsync(web3: Web3, varaTokenDeployment: VaraTokenDeployment): Task<string> =
            web3.Eth.GetContractDeploymentHandler<VaraTokenDeployment>().SendRequestAsync(varaTokenDeployment)
        
        static member DeployContractAndGetServiceAsync(web3: Web3, varaTokenDeployment: VaraTokenDeployment, ?cancellationTokenSource : CancellationTokenSource) = async {
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            let! receipt = VaraTokenService.DeployContractAndWaitForReceiptAsync(web3, varaTokenDeployment, cancellationTokenSourceVal) |> Async.AwaitTask
            return new VaraTokenService(web3, receipt.ContractAddress);
            }
    
        member this.AllowanceQueryAsync(allowanceFunction: AllowanceFunction, ?blockParameter: BlockParameter): Task<BigInteger> =
            let blockParameterVal = defaultArg blockParameter null
            this.ContractHandler.QueryAsync<AllowanceFunction, BigInteger>(allowanceFunction, blockParameterVal)
            
        member this.ApproveRequestAsync(approveFunction: ApproveFunction): Task<string> =
            this.ContractHandler.SendRequestAsync(approveFunction);
        
        member this.ApproveRequestAndWaitForReceiptAsync(approveFunction: ApproveFunction, ?cancellationTokenSource : CancellationTokenSource): Task<TransactionReceipt> =
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            this.ContractHandler.SendRequestAndWaitForReceiptAsync(approveFunction, cancellationTokenSourceVal);
        
        member this.BalanceOfQueryAsync(balanceOfFunction: BalanceOfFunction, ?blockParameter: BlockParameter): Task<BigInteger> =
            let blockParameterVal = defaultArg blockParameter null
            this.ContractHandler.QueryAsync<BalanceOfFunction, BigInteger>(balanceOfFunction, blockParameterVal)
            
        member this.DecimalsQueryAsync(decimalsFunction: DecimalsFunction, ?blockParameter: BlockParameter): Task<byte> =
            let blockParameterVal = defaultArg blockParameter null
            this.ContractHandler.QueryAsync<DecimalsFunction, byte>(decimalsFunction, blockParameterVal)
            
        member this.DecreaseAllowanceRequestAsync(decreaseAllowanceFunction: DecreaseAllowanceFunction): Task<string> =
            this.ContractHandler.SendRequestAsync(decreaseAllowanceFunction);
        
        member this.DecreaseAllowanceRequestAndWaitForReceiptAsync(decreaseAllowanceFunction: DecreaseAllowanceFunction, ?cancellationTokenSource : CancellationTokenSource): Task<TransactionReceipt> =
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            this.ContractHandler.SendRequestAndWaitForReceiptAsync(decreaseAllowanceFunction, cancellationTokenSourceVal);
        
        member this.IncreaseAllowanceRequestAsync(increaseAllowanceFunction: IncreaseAllowanceFunction): Task<string> =
            this.ContractHandler.SendRequestAsync(increaseAllowanceFunction);
        
        member this.IncreaseAllowanceRequestAndWaitForReceiptAsync(increaseAllowanceFunction: IncreaseAllowanceFunction, ?cancellationTokenSource : CancellationTokenSource): Task<TransactionReceipt> =
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            this.ContractHandler.SendRequestAndWaitForReceiptAsync(increaseAllowanceFunction, cancellationTokenSourceVal);
        
        member this.NameQueryAsync(nameFunction: NameFunction, ?blockParameter: BlockParameter): Task<string> =
            let blockParameterVal = defaultArg blockParameter null
            this.ContractHandler.QueryAsync<NameFunction, string>(nameFunction, blockParameterVal)
            
        member this.SymbolQueryAsync(symbolFunction: SymbolFunction, ?blockParameter: BlockParameter): Task<string> =
            let blockParameterVal = defaultArg blockParameter null
            this.ContractHandler.QueryAsync<SymbolFunction, string>(symbolFunction, blockParameterVal)
            
        member this.TotalSupplyQueryAsync(totalSupplyFunction: TotalSupplyFunction, ?blockParameter: BlockParameter): Task<BigInteger> =
            let blockParameterVal = defaultArg blockParameter null
            this.ContractHandler.QueryAsync<TotalSupplyFunction, BigInteger>(totalSupplyFunction, blockParameterVal)
            
        member this.TransferRequestAsync(transferFunction: TransferFunction): Task<string> =
            this.ContractHandler.SendRequestAsync(transferFunction);
        
        member this.TransferRequestAndWaitForReceiptAsync(transferFunction: TransferFunction, ?cancellationTokenSource : CancellationTokenSource): Task<TransactionReceipt> =
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            this.ContractHandler.SendRequestAndWaitForReceiptAsync(transferFunction, cancellationTokenSourceVal);
        
        member this.TransferFromRequestAsync(transferFromFunction: TransferFromFunction): Task<string> =
            this.ContractHandler.SendRequestAsync(transferFromFunction);
        
        member this.TransferFromRequestAndWaitForReceiptAsync(transferFromFunction: TransferFromFunction, ?cancellationTokenSource : CancellationTokenSource): Task<TransactionReceipt> =
            let cancellationTokenSourceVal = defaultArg cancellationTokenSource null
            this.ContractHandler.SendRequestAndWaitForReceiptAsync(transferFromFunction, cancellationTokenSourceVal);
        
    

