from flask import Flask, request, jsonify
import pandas as pd
from sklearn.linear_model import LinearRegression
import datetime
import random

app = Flask(__name__)

# 1. TAHMİN MOTORU (Linear Regression)
@app.route('/tahmin_et', methods=['POST'])
def tahmin_et():
    try:
        # C#'tan gelen veriyi al
        data = request.json
        if not data:
            return jsonify({"tahminler": []})

        df = pd.DataFrame(data)
        df['Tarih'] = pd.to_datetime(df['Tarih'])
        
        # Tarihi sayısal 'Gün' değerine çevir (Regresyon için X ekseni)
        baslangic_tarihi = df['Tarih'].min()
        df['Gun'] = (df['Tarih'] - baslangic_tarihi).dt.days 
        
        # X: Günler, y: Miktarlar
        X = df[['Gun']]
        y = df['Miktar']
        
        # Modeli Eğit
        model = LinearRegression()
        model.fit(X, y)
        
        # Gelecek 7 günü tahmin et (Son günden itibaren)
        son_gun = df['Gun'].max()
        gelecek_gunler = []
        # Yarından itibaren 7 gün (1, 2, 3...7)
        for i in range(1, 8):
            gelecek_gunler.append([son_gun + i])
            
        tahmin_sonuclari = model.predict(gelecek_gunler)
        
        sonuc_listesi = []
        tur_adi = df['Tur'].iloc[0] if 'Tur' in df.columns else "Genel"

        for i, miktar in enumerate(tahmin_sonuclari):
            # Tahmin edilen günü tekrar tarihe çevir
            tahmin_tarihi = baslangic_tarihi + datetime.timedelta(days=int(gelecek_gunler[i][0]))
            
            # Negatif değer çıkarsa 0 yap (Eksi atık olamaz)
            tahmin_kg = max(0, round(miktar, 1))

            sonuc_listesi.append({
                "Tarih": tahmin_tarihi.strftime("%Y-%m-%d"),
                "TahminKG": tahmin_kg,
                "Tur": tur_adi
            })
            
        print(f"✅ Tahmin Yapıldı: {tur_adi}")
        return jsonify({"tahminler": sonuc_listesi})

    except Exception as e:
        print(f"❌ Hata: {str(e)}")
        return jsonify({"error": str(e)}), 500

# 2. ANLIK PİYASA VERİSİ
@app.route('/piyasa_verisi', methods=['GET'])
def piyasa_verisi():
    fiyat = 45.50 + random.uniform(-0.5, 0.5)
    return jsonify({"mazot_fiyati": round(fiyat, 2)})

if __name__ == '__main__':
    print("🚀 YAPAY ZEKA SERVİSİ BAŞLATILDI (Port: 5000)")
    app.run(port=5000)