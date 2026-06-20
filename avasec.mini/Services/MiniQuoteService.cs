namespace AVASec.Mini.Services;

public sealed class DailyQuote
{
    public string Icon { get; init; } = "💚";
    public string TextVi { get; init; } = string.Empty;
    public string TextEn { get; init; } = string.Empty;
    public string AuthorVi { get; init; } = string.Empty;
    public string AuthorEn { get; init; } = string.Empty;
}

public static class MiniQuoteService
{
    private static readonly (string icon, string vi, string en, string authorVi, string authorEn)[] Quotes =
    [
        ("💧", "Uống đủ nước giúp não minh mẫn và da khỏe mạnh.", "Drink enough water for a clear mind and healthy skin.", "Sức khỏe / Health", "Health tip"),
        ("🚶", "Đi bộ 15 phút mỗi ngày cũng đủ để cải thiện tâm trạng.", "A 15-minute walk daily can lift your mood.", "Vận động / Exercise", "Exercise tip"),
        ("😊", "Một nụ cười nhẹ nhàng có thể làm bớt căng thẳng ngay lập tức.", "A gentle smile can ease stress instantly.", "Tâm lý / Wellness", "Wellness tip"),
        ("🌿", "Nghỉ mắt 20 giây sau mỗi 20 phút nhìn màn hình.", "Rest your eyes 20 seconds every 20 minutes of screen time.", "Mắt / Eyes", "Eye care"),
        ("🍎", "Ăn sáng đủ chất giúp bạn khởi đầu ngày mới tràn năng lượng.", "A balanced breakfast fuels your morning energy.", "Dinh dưỡng / Nutrition", "Nutrition tip"),
        ("🧘", "Hít thở sâu 5 lần trước khi làm việc để giữ bình an.", "Take 5 deep breaths before work to stay calm.", "Thiền / Mindfulness", "Mindfulness"),
        ("😴", "Ngủ đủ 7-8 tiếng giúp cơ thể phục hồi tốt hơn.", "7-8 hours of sleep helps your body recover well.", "Giấc ngủ / Sleep", "Sleep tip"),
        ("🤝", "Chia sẻ niềm vui nhỏ với người thân để tăng hạnh phúc.", "Share small joys with loved ones to boost happiness.", "Tình cảm / Connection", "Connection tip"),
        ("📵", "Tắt thông báo 10 phút để tập trung và thư giãn.", "Mute notifications for 10 minutes to focus and relax.", "Cân bằng / Balance", "Balance tip"),
        ("🌞", "Ra nắng sáng sớm giúp cơ thể tổng hợp vitamin D tự nhiên.", "Morning sunlight helps your body make natural vitamin D.", "Sức khỏe / Health", "Health tip"),
        ("🎵", "Nghe một bài hát yêu thích để nạp lại năng lượng tích cực.", "Listen to a favorite song to recharge positive energy.", "Tinh thần / Spirit", "Spirit tip"),
        ("🧹", "Không gian gọn gàng giúp tâm trí nhẹ nhàng hơn.", "A tidy space makes your mind feel lighter.", "Môi trường / Environment", "Environment tip"),
        ("💪", "Mỗi bước nhỏ hôm nay là nền tảng cho sức khỏe mai.", "Every small step today builds tomorrow's health.", "Động lực / Motivation", "Motivation"),
        ("🫖", "Uống trà ấm và thở chậm — quà nhỏ cho bản thân.", "Warm tea and slow breathing — a small gift to yourself.", "Thư giãn / Relax", "Relax tip"),
        ("🌸", "Hãy tử tế với chính mình như bạn tử tế với bạn bè.", "Be as kind to yourself as you are to your friends.", "Tự yêu thương / Self-care", "Self-care")
    ];

    public static DailyQuote GetToday()
    {
        int index = Math.Abs(DateTime.Now.Date.GetHashCode()) % Quotes.Length;
        var q = Quotes[index];
        return new DailyQuote
        {
            Icon = q.icon,
            TextVi = q.vi,
            TextEn = q.en,
            AuthorVi = q.authorVi,
            AuthorEn = q.authorEn
        };
    }
}