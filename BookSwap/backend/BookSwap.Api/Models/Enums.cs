namespace BookSwap.Api.Models;

public enum BookCondition { New = 1, Excellent = 2, Good = 3, Fair = 4, Poor = 5 }
public enum DealType { Exchange = 1, GiveAway = 2, Sell = 3, Lend = 4 }
public enum BookStatus { Available = 1, Reserved = 2, Exchanged = 3, Hidden = 4 }
public enum ExchangeStatus { Pending = 1, Accepted = 2, Rejected = 3, Cancelled = 4, Completed = 5 }
public enum ComplaintStatus { Open = 1, InReview = 2, Resolved = 3, Rejected = 4 }
public enum NotificationType { Exchange = 1, Message = 2, Review = 3, Moderation = 4, System = 5 }
