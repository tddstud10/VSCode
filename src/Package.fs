[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module TddStud10.VSCode.Package

open Fable.Import.vscode
open TddStud10.VSCode.Package
open Ionide.VSCode.Helpers

let activate (_ : Disposable []) = 
    Engine.start()
    |> Promise.map (fun _ -> printf ">>> Just loaded TddStud10...")
    |> ignore
    commands.registerCommand 
        ("tddStud10.enable", (fun () -> window.showInformationMessage "Enabled TddStud10" |> ignore) |> unbox) 
    |> ignore
    commands.registerCommand 
        ("tddStud10.disable", (fun () -> window.showInformationMessage "Disabled TddStud10" |> ignore) |> unbox) 
    |> ignore

let deactivate (_ : Disposable []) = Engine.stop()
