import { useTranslation } from 'react-i18next';
import PageTemplate from '../../components/PageTemplate/PageTemplate';
import './RulesPage.scss';

function RulesPage() {
  const { t } = useTranslation();
  
  return (
    <PageTemplate title="Säännöt">
      <div className="rules-container">
        <h1 className="rules-title">MAHL<br />YLEISSÄÄNTÖJÄ</h1>
        
        <section className="rule-section">
          <div className="rule-number">01.</div>
          <div className="rule-content">
            <h2>YLEISTÄ</h2>
            <p>
              Pelaaja tai maalivahti voi edustaa toimintakauden aikana vain yhtä 
              MAHL harrasteliigan joukkueetta per laji (HUOM! lajikohtaiset säännöt 
              voivat kumota em. säännön). Mikäli pelaajan tai maalivahdin joukkue 
              luopuu sarjapaikasta harrasteliigassa voi hän vapaasti valita uuden 
              joukkueen, jota edustaa.
            </p>
            <p>
              Juniorisarjoissa edustusoikeudesta vastaa lajivastaavat. Jos 
              junioripelaaja haluaa siirtyä joukkueesta toiseen, tulee 
              joukkueenjohtajien ottaa yhteyttä lajivastaaviin asian selvittämiseksi. 
              Lajivastaavan päätös edustusoikeudesta kumoaa mahdolliset sanktiot!
            </p>
          </div>
        </section>

        <section className="rule-section">
          <div className="rule-number">02.</div>
          <div className="rule-content">
            <h2>TOIMINTAKAUSI JA<br />REKISTERÖITYMISMAKSU</h2>
            <p>MAHL:n toimintakausi on 1.5.-30.4.</p>
            <p>
              Pelaajan ja maalivahdin on maksettava rekisteröitymismaksu 
              toimintakaudesta MAHL:lle. Rekisteröitymismaksun suuruus on 30 € ja 
              se kattaa kaikki MAHL toimintakauden harrasteliigat. Vasta kyseisen 
              maksun suorittamisen jälkeen on pelaaja tai maalivahti oikeutettu 
              edustamaan joukkuetta MAHL:n harrasteliigassa.
            </p>
            <p>
              MAHL:n järjestämissä junioriharrasteliigoissa rekisteröitymismaksu on 
              joukkuekohtainen ja on toimintakaudesta 50 €, junioriliigoissa 
              rekisteröitymismaksu sisällytetään poikkeuksetta osallistumismaksuun. 
              Junioriliigat ovat alle 18- vuotiaille järjestettyjä liigoja.
            </p>
            <p>
              MAHL:n järjestämissä turnauksissa rekisteröintimaksu sisältyy 
              turnauksen joukkuemaksuun.
            </p>
          </div>
        </section>

        <section className="rule-section">
          <div className="rule-number">03.</div>
          <div className="rule-content">
            <h2>VAKUUTUS JA LISENSSI</h2>
            <p>
              MAHL ei vastaa mahdollisista loukkaantumisista tai muutenkaan 
              henkilöiden vakuutusturvasta.
            </p>
            <p>
              Mikäli MAHL:n harrasteliiga on kyseisen lajin lajiliiton alainen laji, 
              pelaaja ja maalivahti on velvollinen ottamaan kyseisen lajiliiton 
              lisenssin ja hoitamaan lajiliiton asettamat velvoitteet vakuutusturvasta.
            </p>
          </div>
        </section>

        <section className="rule-section">
          <div className="rule-number">04.</div>
          <div className="rule-content">
            <h2>EDUSTUSOIKEUS JA SANKTIOT</h2>
            <p>
              Ennen kauden alkua on joukkueen toimitettava MAHL:lle joukkueensa 
              pelaajaluettelo. Pelaajaluettelossa voi olla ainoastaan rekisteröityjä 
              pelaajia tai maalivahteja.
            </p>
            <p>
              Mikäli harrasteliiga on lajiliiton alainen laji niin pelaajilla ja 
              maalivahdeilla täytyy olla lisenssi maksettuna sekä vakuutusturva 
              hoidettuna.
            </p>
            <p>
              Mikäli joukkueessa pelaa pelaaja tai maalivahti, joka ei ole suorittanut 
              rekisteröitymismaksua tai lisenssiä, tuomitaan joukkue hävinneeksi 
              kyseisen ottelun 5-0 tai maalieron / piste eron ollessa suurempi, niin 
              kyseisellä tuloksella. Tämän lisäksi tulee joukkueen suorittaa MAHL:lle 
              50 € suuruinen sanktiomaksu.
            </p>
            <p>
              Mikäli joukkue ei suorita velvoitteitaan MAHL kohtaan niin MAHL voi 
              sulkea kyseisen joukkueen kaikki rekisteröityneet pelaajat ja maalivahdit 
              MAHL järjestämistä harrasteliigasarjoista.
            </p>
            <p>
              Mikäli pelaaja, maalivahti tai toimihenkilö syyllistyy epäurheilijamaiseen 
              käytökseen ottelutapahtuman aikana, sitä ennen tai sen jälkeen, sen lisäksi 
              mitä sanktioita mahdollinen lajiliitto kyseiselle henkilölle määrää, 
              pidättää MAHL oikeuden sulkea kyseinen pelaaja MAHL harrasteliigoista.
            </p>
          </div>
        </section>

        <section className="rule-section">
          <div className="rule-number">05.</div>
          <div className="rule-content">
            <h2>OTTELUN PERUMINEN</h2>
            <p>
              Mikäli joukkue peruu ilman hyväksyttävää syytä ottelun tai jättää 
              tulematta paikalla, tuomitaan joukkueelle ottelusta koituvien 
              kustannusten mukainen korvaus.
            </p>
            <p>MAHL ry päättää onko syy pelin perumiselle hyväksyttävä.</p>
          </div>
        </section>

        <section className="rule-section">
          <div className="rule-number">06.</div>
          <div className="rule-content">
            <h2>MUUTA</h2>
            <p>
              MAHL:n järjestämissä liigoissa ja turnauksissa esiintyvien henkilöiden 
              tulee olla täys-ikäisiä.
            </p>
            <p>
              MAHL ry pidättää oikeuden sääntömuutoksiin kesken toimintakauden.
            </p>
            <p>
              Eri harrasteliigojen lajikohtaisia sääntöjä tarkennetaan kyseisen lajin 
              kohdalla.
            </p>
          </div>
        </section>
      </div>
    </PageTemplate>
  );
}

export default RulesPage; 