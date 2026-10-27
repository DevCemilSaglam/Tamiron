namespace Tamiron.Domain;

/// <summary>K-006, K-026.3. <c>Both</c> bakım sayılır: km sorulur, hatırlatma kurulur.</summary>
public enum ServiceType { Repair = 1, Maintenance = 2, Both = 3 }

/// <summary>Adım 7. <c>Unpaid</c> = "Almadım" — bir ödeme yoludur (K-004), alacağın kaynağı.</summary>
/// <summary>Advance: müşteri avansından düşüldü (G7, K-079.1) — kalanı kapatır, kasaya girmez.</summary>
public enum PaymentMethod { Cash = 1, Card = 2, Transfer = 3, Unpaid = 4, Advance = 5 }

public enum LineKind { Part = 1, Labor = 2 }

/// <summary>K-005 sorumluluk anahtarı. Yalnız parça satırında anlamlı.</summary>
public enum PartSupplier { Shop = 1, Customer = 2 }

/// <summary>Sahip tipi (C6, KDV.md §2): tevkifat bireyselde YOK, şirket/kurumda var.</summary>
public enum OwnerType { Individual = 1, Company = 2 }

/// <summary>PARCA-TANIMLAMA.md §3.</summary>
public enum MatchStatus { Unidentified = 0, Coded = 1, Matched = 2 }
