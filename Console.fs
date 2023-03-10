module Console

open System
open System.Reflection

let log =
    let lockObj = obj()
    fun color s ->
        lock lockObj (fun _ ->
            Console.ForegroundColor <- color
            printfn "%s" s
            Console.ResetColor())

let complete = log ConsoleColor.Magenta
let ok = log ConsoleColor.Green
let info = log ConsoleColor.Cyan
let warn = log ConsoleColor.Yellow
let error = log ConsoleColor.Red
let debug x = 
    x |> sprintf "%A" |> log ConsoleColor.Gray
    x
let dbg x =
    x |> sprintf "%A" |> log ConsoleColor.DarkRed
    x