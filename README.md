# csharp_read_vcf
c# reader for vcf files

Utworzenie klasy/bilbioteki w c#, którą można wykorzystać jako importer dla plikow vcf o strukturze:
http://www.1000genomes.org/wiki/Analysis/Variant%20Call%20Format/vcf-variant-call-format-version-40/
http://gatkforums.broadinstitute.org/gatk/discussion/1268/what-is-a-vcf-and-how-should-i-interpret-it

pole pliku "INFO" może posiadać dodatkowe klasy takie jak ANN/EFF opisane na stronie (6. Additional annotations):
http://snpeff.sourceforge.net/SnpEff_manual.html#effNc
http://snpeff.sourceforge.net/VCFannotationformat_v1.0.pdf

Ważne jest, żeby plik wczytywał się w 2 różnych częściach (header, records) zgodnie ze schematem (2. Basic structure of a VCF file na http://gatkforums.broadinstitute.org/gatk/discussion/1268/what-is-a-vcf-and-how-should-i-interpret-it)

nagłówek oznaczony jest jako ##
nagłówek poszczególnych rekordów/próbek oznaczony jest jako # (część stała dla rekordów danych i zmienna dla liczby próbek - im więcej próbek tym więcej kolumn)
rekordy odzielane są znakiem "/t", część INFO odzielana jest różnie w zależności od rekordu i czy posiada wartości, najczęściej znakiem ";", część przed = to nazwa kolumny/zmiennej (wewnątrz kolumn ANN/EFF -> odzielenie "|")



