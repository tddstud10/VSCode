[<ReflectedDefinition>]
module TddStud10.VSCode

open System
open System.Text.RegularExpressions
open Fable.Core
open Fable.Import.vscode
open Ionide
open Ionide.VSCode

module PackageService =
    open Fable.Import.Node.child_process_types
    open Ionide.VSCode.Helpers
    open Fable.Import.Node

    type BuildData = {Name : string; Start : DateTime; mutable End : DateTime option; Process : ChildProcess}

    let mutable private linuxPrefix = ""
    let mutable private command = ""
    let mutable private script = ""
    let mutable private BuildList = ResizeArray()
    let outputChannel = window.createOutputChannel "FAKE"

    let private loadParameters () =
        let p = workspace.rootPath
        linuxPrefix <- Settings.loadOrDefault (fun s -> s.Fake.linuxPrefix ) "sh"
        command <- Settings.loadOrDefault (fun s -> p + "/" + s.Fake.command ) (if Process.isWin () then p + "/" + "build.cmd" else p + "/" + "build.sh")
        script <- Settings.loadOrDefault (fun s -> p + "/" + s.Fake.build )  (p + "/" + "build.fsx")
        ()

    let private startBuild target =
        if JS.isDefined target then
            outputChannel.clear ()
            window.showInformationMessage ("Build started", "Open")
            |> Promise.success(fun n -> if n = "Open" then outputChannel.show (2 |> unbox) )
            |> ignore
            let proc = Process.spawnWithNotification command linuxPrefix target outputChannel
            let data = {Name = (if target = "" then "Default" else target); Start = DateTime.Now; End = None; Process = proc}
            BuildList.Add data
            let cfg = workspace.getConfiguration ()
            if cfg.get("FAKE.autoshow", true) then outputChannel.show ()
            proc.on("exit",unbox<Fable.Import.JS.Function>(fun (code : string) ->
                if code ="0" then
                    window.showInformationMessage "Build completed" |> ignore
                else
                    window.showErrorMessage "Build failed" |> ignore
                data.End <- Some DateTime.Now)) |> ignore


    let cancelBuild target =
        let build = BuildList |> Seq.find (fun t -> t.Name = target)
        if Process.isWin () then
            Process.spawn "taskkill" "" ("/pid " + build.Process.pid.ToString() + " /f /t")
            |> ignore
        else
            build.Process.kill ()
        build.End <- Some DateTime.Now

    let buildHandle () =
        do loadParameters ()

        script
        |> fs.readFileSync
        |> fun n -> (n.toString(), "Target \"([^\".]+)\"")
        |> Regex.Matches
        |> Seq.cast<Match>
        |> Seq.toArray
        |> Array.map(fun m -> m.Groups.[1].Value)
        |> fun a -> ResizeArray(a)
        |> Promise.lift
        |> Case2
        |> window.showQuickPick
        |> Promise.success startBuild

    let cancelHandle () =
        let targets =
            BuildList
            |> Seq.where (fun n -> n.End.IsNone)
            |> Seq.map (fun n -> n.Name)
            |> Seq.toArray

        if Array.length targets = 1 then
            targets.[0]
            |> Promise.lift
            |> Promise.success cancelBuild
        else
            targets
            |> fun a -> ResizeArray(a)
            |> Promise.lift
            |> Case2
            |> window.showQuickPick
            |> Promise.success cancelBuild

    let defaultHandle () =
        do loadParameters ()
        do startBuild ""

type Package() =
    member x.activate(state:obj) =
        commands.registerCommand("tddStud10.fakeBuild", PackageService.buildHandle |> unbox ) |> ignore
        commands.registerCommand("tddStud10.cancelBuild", PackageService.cancelHandle |> unbox) |> ignore
        commands.registerCommand("tddStud10.buildDefault", PackageService.defaultHandle |> unbox) |> ignore
        ()
