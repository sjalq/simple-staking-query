module CsvFormat

open System
open System.Numerics
open System.IO

let toFile map =
    let timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss")
    let filePath = sprintf "results %s.csv" timestamp

    File.WriteAllLines(
        filePath,
        map
        |> Map.map (fun k v -> sprintf "%A,%A" k v)
        |> Map.values
    )
