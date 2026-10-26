# Chillquarium Demo Yol Haritası

Amaç: [Chillquarium](https://play.google.com/store/apps/details?id=com.snapbreak.chillquarium) oyununun demosunu yaparak oyun yapmayı öğrenmek. Küçük adımlarla başlanır, sonra büyük sistemler kurulur. Her adım işe başlarken ayrıca detaylı planlanır.

## Görsel stil

Referans oyunun görsel stiliyle aynı. Tam pixel art değil: balık ve dekorlarda piksel dokusu ve kalın, koyu dış çizgi var; UI düz, yuvarlak köşeli ve yuvarlak, kalın bir font kullanıyor. Ekran görüntüleri `docs/references/` altında (`steam-*` 1920×1080, `play-*` 900 px). Bütün görselleri Codex CLI ile kendimiz üretiriz; hazır asset paketi kullanılmaz. Mekanik adımlarında geçici görseller kullanılır, gerçek görseller 13. adımda gelir.

## Yapılacaklar

- [x] **1. Akvaryum sahnesi:** kamera, arka plan ve balıkların yüzeceği alanın sınırları.
- [x] **2. İlk balık:** geçici bir balık görseli, akvaryumda rastgele yüzüyor ve kenara gelince dönüyor.
- [x] **3. Balık verisi:** her türün adı, görseli, nadirliği, baby ve adult gelirleri tek yerde tutulur. Başlangıçta 4–6 tür.
- [ ] **4. Nadirlik:** common, rare, epic, legendary. Her birinin rengi ve gelir farkı olur.
- [ ] **5. Para:** cüzdan ve ekranda görünen para göstergesi.
- [ ] **6. Idle gelir:** her balık belli aralıklarla kendiliğinden para üretir. Baby az, adult çok getirir.
- [ ] **7. Büyüme:** balık zamanla baby'den adult'a geçer, görünüşü ve boyu değişir.
- [ ] **8. Besleme:** yem atılır, balıklar yemi yer ve daha hızlı büyür (referanstaki "feeding frenzy").
- [ ] **9. Market:** tek balık kartı, fiyatı ve satın alma butonu.
- [ ] **10. Kart açma:** nadirlik olasılıklarına göre rastgele bir balık çıkar. Basit bir açılış ekranından sonra balık baby olarak akvaryuma eklenir.
- [ ] **11. Kayıt:** para, balıklar ve büyüme durumu saklanır. Oyun kapatılıp açılınca kaldığı yerden devam eder.
- [ ] **12. Denge:** fiyatlar, gelirler, büyüme süreleri ve nadirlik oranları tek bir ayar dosyasında toplanır, oynanarak ayarlanır.
- [ ] **13. Görseller:** referans stilde balık türleri (baby ve adult), akvaryum arka planı ve UI, Codex CLI ile üretilir.
- [ ] **14. Cila:** baloncuklar, para çıkış animasyonu, ses gibi küçük dokunuşlar.
