[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module TddStud10.VSCode.Package

open Fable.Import.vscode
open TddStud10.VSCode.Package
open Ionide.VSCode.Helpers

module Linter =
    open Fable.Core.JsInterop

    let mutable engineRunning = ref false

    let fsw = workspace.createFileSystemWatcher("**/*")

    let handler eType (uri : Uri) =
        printf ">>> FSW Event %s - %s" eType uri.fsPath
        if uri.fsPath.IndexOf(@"\.git\", System.StringComparison.OrdinalIgnoreCase) >= 0 then
            printf ">>> Ignoring - path filtered out..." |> Promise.lift
        elif !engineRunning then
            printf ">>> Ignoring - engine running already..." |> Promise.lift
        else
            printf ">>> Setting engineRunning to true..."
            engineRunning := true
            Engine.run ()
            |> Promise.onFail (fun r ->
                printf ">>> Setting engineRunning to false."
                engineRunning := false
            )
            |> Promise.bind(fun r ->
                printf ">>> Setting engineRunning to false."
                engineRunning := false
                r |> Promise.lift
            )

    let activate (disposables: Disposable[]) =
        fsw.onDidCreate $ (handler "create", (), disposables) |> ignore
        fsw.onDidChange $ (handler "change", (), disposables) |> ignore
        fsw.onDidDelete $ (handler "delete", (), disposables) |> ignore

let activate (disposables : Disposable []) = 
    Engine.start()
    |> Promise.map (fun _ -> 
                    Linter.activate disposables
                    printf ">>> Started Engine...")
    |> Promise.catch (fun error ->
                      printf "Encountered error starting engine %O" error
                      promise { () }) // prevent unhandled rejected promises
    |> ignore
    commands.registerCommand 
        ("tddStud10.enable", (fun () -> window.showInformationMessage "Enabled TddStud10" |> ignore) |> unbox) 
    |> ignore
    commands.registerCommand 
        ("tddStud10.disable", (fun () -> window.showInformationMessage "Disabled TddStud10" |> ignore) |> unbox) 
    |> ignore

let deactivate (_ : Disposable []) = Engine.stop()
