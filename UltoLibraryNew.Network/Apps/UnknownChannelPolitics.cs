namespace UltoLibraryNew.Network.Apps;

public enum UnknownChannelPolitics {
    Skip,
    [Obsolete("Использование этого действия небезопасно, так как может вызывать атаки спамом каналов")]
    CreateNew,
    Disconnect
}