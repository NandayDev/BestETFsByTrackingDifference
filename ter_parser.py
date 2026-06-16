import pandas as pd
import re

# File input
CSV_INPUT = "wwwroot/etfs.csv"
CSV_OUTPUT = "wwwroot/etfs2.csv"
HTML_FILE = "etf.html"

# Legge l'HTML
with open(HTML_FILE, "r", encoding="utf-8", errors="ignore") as f:
    html = f.read()

# Costruisce mappa ISIN -> TER
isin_to_ter = {}

for match in re.finditer(
    r'"ter":"([^"]+)".*?"isin":"([^"]+)"|'
    r'"isin":"([^"]+)".*?"ter":"([^"]+)"',
    html,
    re.DOTALL
):
    if match.group(1):
        ter = match.group(1)
        isin = match.group(2)
    else:
        isin = match.group(3)
        ter = match.group(4)

    isin_to_ter[isin] = ter

# Legge il CSV
df = pd.read_csv(CSV_INPUT, sep=";")

# Aggiunge colonna TER
df["TER"] = df["Isin"].map(isin_to_ter)

# Salva
df.to_csv(CSV_OUTPUT, sep=";", index=False)

print(f"Trovati {len(isin_to_ter)} ETF nell'HTML")
print(f"Creato {CSV_OUTPUT}")