module CsvFormat

open System
open System.Numerics
open System.IO

let toFile map =
    let timestamp = DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss")
    let filePath = sprintf "results %s.csv" timestamp

    File.WriteAllLines(
        filePath,
        map
        |> Map.map (fun k v -> sprintf "%A,%A" k v)
        |> Map.values
    )
