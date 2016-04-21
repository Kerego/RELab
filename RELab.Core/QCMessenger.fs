namespace RELab.Core
open System.Net
open System.Net.Sockets
open System.Text
open System.Text.RegularExpressions

type Status =
    | Online = 0
    | Away = 20
    | DoNotDisturb = 10
    | Offline = 30

type Message() = 
    abstract member Payload : string 
    default this.Payload = ""

type PublicMessage(Channel : string, Sender : string, Content : string) = 
    inherit Message()
    member x.Channel = Channel
    member x.Sender = Sender
    member x.Content = Content
    override x.Payload = "2" + Channel + string '\000' + Sender + string '\000' + Content + string '\000'

type TopicMessage(Topic : string, Sender : string) =
    inherit Message()
    member x.Topic = Topic
    member x.Sender = Sender
    override x.Payload = "B" + Topic + " (" + Sender + ")" + string '\000'

type PrivateMessage(Sender : string, Receiver : string, Content : string) = 
    inherit Message()
    member x.Sender = Sender
    member x.Receiver = Receiver
    member x.Content = Content
    override x.Payload = "J2" + Sender + string '\000' + Receiver + string '\000' + Content + string '\000'

type ConnectMessage(Channel : string, Sender : string) = 
    inherit Message()
    member x.Channel = Channel
    member x.Sender = Sender
    override x.Payload = "4" + Sender + string '\000' + Channel + string '\000' + "20" + string '\000'

type RenameMessage(Sender : string, Nickname : string) = 
    inherit Message()
    member x.Sender = Sender
    member x.Nickname = Nickname
    override x.Payload = "3" + Sender + string '\000' + Nickname + string '\000' + "0"

type ExitMessage(Channel : string, Sender : string) = 
    inherit Message()
    member x.Channel = Channel
    member x.Sender = Sender
    override x.Payload = "5" + Sender + string '\000' + Channel + string '\000' + "0" + string '\000'

type StatusMessage(Sender : string, Status : Status) = 
    inherit Message()
    member x.Status = Status
    member x.Sender = Sender
    override x.Payload = "D" + Sender + string '\000' + (int Status).ToString().PadLeft(2, '0')

type QCMessenger(listenEndPoint : EndPoint, broadcastEndPoint : EndPoint) = 
    let listen = listenEndPoint
    let broadcast = broadcastEndPoint
    let listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
    let broadcastSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
    let buffer : array<byte> = int System.UInt16.MaxValue |> Array.zeroCreate

    do listenSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true)
    do listenSocket.Bind(listenEndPoint)
    do listenSocket.EnableBroadcast = true |> ignore
    do broadcastSocket.EnableBroadcast = true |> ignore


    let Parse (content : string) : Message = 
        let firstChar : char = content.ToCharArray() |> Array.head
        match firstChar with
        | '2' ->    let pattern =  "2(.+?)" + string '\000' + "(.+?)" + string '\000' + "(.+?)" + string '\000'
                    let result = Regex.Match(content, pattern)
                    upcast PublicMessage(result.Groups.[1].Value, result.Groups.[2].Value, result.Groups.[3].Value)
        | '3' ->    let pattern =  "3(.+?)" + string '\000' + "(.+?)" + string '\000' + "0"
                    let result = Regex.Match(content, pattern)
                    upcast RenameMessage(result.Groups.[1].Value, result.Groups.[2].Value)
        | '4' ->    let pattern =  "4(.+?)" + string '\000'+ "(.+?)" + string '\000' + "20" + string '\000'
                    let result = Regex.Match(content, pattern)
                    upcast ConnectMessage(result.Groups.[2].Value, result.Groups.[1].Value)
        | '5' ->    let pattern =  "5(.+?)" + string '\000'+ "(.+?)" + string '\000' + "0" + string '\000'
                    let result = Regex.Match(content, pattern)
                    upcast ExitMessage(result.Groups.[2].Value, result.Groups.[1].Value)
        | 'B' ->    let pattern =  "B(.+?) \((.+?)\)" + string '\000'
                    let result = Regex.Match(content, pattern)
                    upcast TopicMessage(result.Groups.[1].Value, result.Groups.[2].Value)
        | 'D' ->    let pattern =  "D(.+?)" + string '\000' + "(.*)"
                    let result = Regex.Match(content, pattern)
                    upcast StatusMessage(result.Groups.[1].Value, System.Enum.Parse(typeof<Status>,result.Groups.[2].Value) :?> Status)
        | 'J' ->    let pattern = "J2(.+?)" + string '\000' + "(.+?)" + string '\000' + "(.+?)" + string '\000'
                    let result = Regex.Match(content, pattern)
                    upcast PrivateMessage(result.Groups.[1].Value, result.Groups.[2].Value, result.Groups.[3].Value)
        | _ ->      Message()

    member x.Listen() = 
        let receivedBytes = listenSocket.ReceiveFrom(buffer, ref listen)
        let content = buffer |> Array.take receivedBytes |> Encoding.Default.GetString
        Parse content

    member x.ListenAsync() = async { return x.Listen() } |> Async.StartAsTask

        
    member x.Send(message : Message) =
        broadcastSocket.SendTo(message.Payload |> Encoding.Default.GetBytes, broadcast) |> ignore

    member x.SendAsync(message : Message) = async { return x.Send(message : Message) } |> Async.StartAsTask

