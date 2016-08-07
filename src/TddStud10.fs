[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Ionide.VSCode.FSharp

open Fable.Import.vscode
open Ionide.VSCode.FSharp
open Ionide.VSCode.Helpers

let activate (_ : Disposable []) = 
    LanguageService.start()
    |> Promise.success (fun _ -> printf ">>> Just loaded TddStud10...")
    |> ignore
    commands.registerCommand 
        ("tddStud10.enable", (fun () -> window.showInformationMessage "Enabled TddStud10" |> ignore) |> unbox) 
    |> ignore
    commands.registerCommand 
        ("tddStud10.disable", (fun () -> window.showInformationMessage "Disabled TddStud10" |> ignore) |> unbox) 
    |> ignore

let deactivate (_ : Disposable []) = LanguageService.stop()
