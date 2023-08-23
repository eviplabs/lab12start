# Threading Lab

Szálak használata egy minimális grafikus felhasználói felületű alkalmazásban.
Ennek a labornak a kiindulási alapja egy alkalmazás, mely gombnyomásra egy "időigényes" műveletet végez (valójában csak 10-szer várakozik 500ms-ot).

Az időigényes műveletek nem futhatnak a felhasználói felület szálján (UI thread), mivel akkor a művelet alatt "befagyna" a felület, ezért mindezt háttérszálon végezzük majd el. Ezen kívül gyakori igény, hogy lehessen látni, hogyan halad a munka, valamint hogy szükség esetén meg lehessen állítani.

## Felkészülés a mérésre

Az otthoni felkészülés az alábbi lépésekből áll:

- A Moodle alatt kiadott anyagok áttekintése.
- Olvasd el ezt a mérési útmutatót, hogy ne a laboron lásd először.
- Olvasd el az alábbi leírást az async-await mintáról:
https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/async/
- A mérés során szükséged lesz a Dispatcher és CancellationToken használatára, így ennek leírását is olvasd el:
https://docs.microsoft.com/en-us/dotnet/standard/threading/cancellation-in-managed-threads
(Elég az elejétől a "Code Example" résszel bezárólag.)
- Töltsd ki a Moodleben a kapcsolódó tesztet.

## A labor dokumentálása, lezárása

A labor közben
- Folyamatosan (legalább minden feladat után) commitolj.
- FONTOS: A feladatok futási eredményéről készíts screenshotot és ezeket sorszámozva mentsd el egy "screenshots" könyvtárba. Olyan screenshotokat készíts, melyen látszik az előre lépés az előző feladathoz képest. (Ha a UI-on semmi változás nem látszik, akkor kihagyhatod a screenshotot.)

A labor végén
- Ne felejtsd el felpusholni a munkádat.
- Githubon hozz létre egy pull requestet, amiben pontosan a laboron elvégzett változások láthatók.

A labor vége, mint határidő a commitok pusholására és a pull request beküldésére vonatkozik.

## 0. feladat: a UI thread blokkolása

(A labor elején, már most hozz létre egy új branchet és utána azon dolgozz, hogy a pull requestet könnyű legyen majd a labor végén létrehozni! És ha az egyetemen kezdted el a munkát, mielőtt elmész, ne felejtsd el felpusholni a változásokat a laborgépről a githubra!)

A letöltött forráskód ennek a feladatnak a megoldását már tartalmazza. Vizsgáld meg a Blocker nyomógomb eseménykezelőjét, hogyan várakozik. Mivel ez a UI threaden fut, amíg le nem fut, a felhasználói felület nem tud reagálni eseményekre. Például a nyomógombok utáni checkbox nem tud reagálni a kattintásokra. Az alapvetően egy nagyon rossz dolog, ha egy programban így működik valami.

## 1. feladat: az alkalmazás kipróbálása

Nézd meg a "Start!" nyomógomb eseménykezelőjét, és általában a példaprogram működését! A Progress osztály képes arra, hogy rajta keresztül egy háttérszál jelezze, hogy hol tart. Típus paramétere most "int", mert egész százalékokat fogunk visszaküldeni. A konstruktor paramétere egy lambda kifejezés, ami azt adja meg, hogy mi történjen, ha report érkezik a háttérszáltól. Jelen esetben a ProgressBar értékét frissítjük.

Mivel a háttérmunkát most a UI threaden végezzük, bár rendszeresen frissítenénk a ProgressBar értékét, az "nem jut szóhoz", így az eredményt csak akkor látjuk, amikor a munka véget ér. Ekkor a ProgressBar lehetőséget kap arra, hogy újrarajzolja magát, akkor már 100%-on állva.

## 2. feladat: async használata

Alakítsd át a DoIt metódust async metódussá (ekkor a visszatérési érték ``Task`` kell, hogy legyen), a "Start!" nyomógomb eseménykezelőjét pedig úgy, hogy ezt hívja meg. Async metódusok nevének a vége mindig Async (kódolási konvenció), így nevezd át DoItAsync-ra. (Átnevezés után Ctrl+. segítségével kérd meg a Visual Studiot, hogy minden hívási helyen írja át a nevét.)

Ahhoz, hogy a ``Start_Click`` metódusban lehessen await, fontos, hogy ő is async metódus legyen. (Ezzel szólunk a fordítónak, hogy olyan metódus kódot kell neki generálni, ami támogatja az async-await mechanizmust.)

Ha az egeret a DoItAsync metódusban ráhúzod a ``Task.Delay(500).Wait();`` Delay-jére, az IntelliSense is mutatja, hogy a metódus "awaitable". Írd át úgy, hogy a végén lévő (blokkoló) ``Wait()`` hívás helyett ennek a lefutását is az ``await`` kulcsszóval várd meg.

Próbáld ki az alkalmazást! Bár a munka még mindig a UI threaden fut (az async hívás csak a futás megszakítását teszi lehetővé, nem rakja át másik szálra), mivel a várakozások közben blokkolás helyett hagyjuk szóhoz jutni a UI threaden a többi eseményt is (pl. UI újrarajolása), a ProgressBar most már szépen frissül menet közben is.

## 3. feladat: az esemény lista frissítése

Egészítsd ki a ``DoItAsync`` metódust, hogy az előre haladás reportolása mellett az ``Events`` lista elemeihoz is adjon hozzá egy új sort, amiben szövegesen leírja, hogy halad és most hány százaléknál tart.

## 4. feladat: visszatérés a blokkoló várakozásra

Tegyük fel, hogy az elvégzendő, időigényes munka tényleg valami számítási feladat és nem csak egy ``Wait``. Ekkor az async-await nem oldaná meg a problémát, mert a UI threaden folyamatosan számolnánk, amit hiába await-elünk, az még folyamatosan dolgozik. Írd vissza a DoItAsync-beli várakozást blokkolóra, ahogy eredetileg is volt.

Ekkor a ProgressBar megint csak a munka legvégén frissül.

## 5. feladat: DoItAsync futtatása háttérszálon

A ``Task.Run`` statikus metódusnak át lehet adni egy lambda kifejezést (paraméterének típusa ``Func<Task>``, vagyis egy kifejezés, ami ``Task``-ot ad vissza, a DoItAsync pont ilyen), és azt egy háttérszálon fogja elindítani.

Indítsd el az alkalmazást!
Valamiért a háttérmunka nem fut rendesen... a Visual Studio "Output" ablakában viszont megjelenik egy hibaüzenet: 

``"Exception thrown: 'System.Exception' in System.Private.CoreLib.dll"``

Valójában a héttérszálon futó kód hibát dobott! Felvetődik a kérdés, hogy akkor a Visual Studio debuggere miért nem állt meg és jelezte ezt. Azért, mert nagyon sok kivételre alapbeállításként a debugger nem áll meg, mert sokszor ez jobban zavarná a hibakeresést, mint segítené. (Folyton megállna a futás, mindenféle apróságok miatt.)

## 6. feladat: az Exception megvizsgálása

Futás közben megjelenik a Visual Studio "Exception settings" ablaka, itt most kapcsold be a Common Language Runtime Exceptions csoport minden elemét. Így újra futtatva az alkalmazást már megáll a debugger a hibaüzenetnél. A gond az eventList kiegészítésénél van:

``System.Exception: The application called an interface that was marshalled for a different thread.
	(Exception from HRESULT: 0x8001010E (RPC_E_WRONG_THREAD))
   at System.Runtime.InteropServices.WindowsRuntime.IVector`1.Append(T value)
   at System.Runtime.InteropServices.WindowsRuntime.VectorToCollectionAdapter.Add[T](T item)
   at ThreadingLab.MainPage.SlowBackgroundProcessor.<DoIt>d__2.MoveNext()``

Elsőre elég ijesztő a szövege. Valójában az van, hogy egy interface (most az ``EventListView`` funkciói) nem érhető el, csak arról a szálról, amin létrehoztuk. Ez minden UI elem esetében így van: UI elemekhez csak a UI threadről lehet hozzáférni. Eddig ezzel nem volt gond, mert minden a UI threaden futott, de most a háttérszálról akarunk egy új elemet felvenni az ``EventListView`` listájára.

A megoldás a következő feladat része. Most, hogy megvan a hiba oka, állítsd vissza az Exception settingst, hogy a CLR-nek csak az alapbeállítás szerinti kivételeinél álljon meg a debugger. (Ehhez a csoport eleji checkboxra kell kattintgatni, amígy újra kis négyzet lesz a kitöltés.)

## 7. feladat: áthívás a UI threadre a háttérszálról

Az előző feladat konklúziója szerint a háttérszálról (a DoItAsync metódusból) az eventList kiegészítését a UI threaden kellene lefuttatni.
Erre való az IDispatcher interface, ami egy szál számára ütemezi a beérkező feladatokat (pl. felhasználói eseményeket). Minden UI elem (így a ``MainPage`` is) rendelkezik egy referenciával a dispatcherjére. A ``MainPage.Dispatcher.DispatchAsync`` metódusnak átadhatsz egy lambda kifejezést, amit ő saját szálján (jelen esetben a UI threaden) fog lefuttatni.

Ha ez sikerül, akkor az Events lista a DoItAsync metódusból is frissíthető lesz és folyamatosan meg fognak jelenni az új bejegyzések. Az Output ablakban pedig nem lesz exception.

Ezen a ponton érdemes megfigyelni, hogy az eseménylista szerint a "Start!" nyomógomb eseménykezelője szinte azonnal teljesen lefut, vagyis nem csak akkor ér véget, amikor a háttérfeladatok mind elkészülnek.

## 8. feladat: A "Start!" többszöri megnyomása

Ez csak egy rövid kísérlet: mivel a Start nyomógomb eseménykezelője háttérszálakat indít, ha a háttérmunka alatt még egyszer megnyomjuk, akkor kétszer fog futni párhuzamosan a DoItAsync. Ennek a ProgressBar számára érdekes hatása van... próbáld ki!

## 9. feladat: két ProgressBar, két háttérfolyamat

Egészítsd ki a "Start!" eseménykezelőjét, hogy két ``SlowBackgroundProcessor`` példányt hozzon létre. Ezekből csak az egyik írjon az Events listába, a másiknak null-t adj át és a DoItAsync metódusban kezeld le, hogy az Events lehet null. Az egyik háttér task az első, a második a második ProgressBar-ra küldje, hogy hol jár.

A két taskot egyszerre (közvetlenül egymás után) indítsd el.

Próbáld ki, hogyan működik! A két ProgressBar egyszerre halad előre.

## 10. feladat: Taskok egymás utáni futtatása

A Task.Run visszatérési értéke Task, aminek pedig van egy csomó hasznos metódusa. Az online dokumentációban nézd meg, melyik az a metódus, amit ha meghívsz a Task.Run visszatérési értékére, akkor megadhatsz egy másik feladatot (lambda kifejezést), amit megint csak egy háttér task-ban fog elindítani, csak éppen nem azonnal, hanem akkor, amikor az első véget ért.

Próbáld ki az alkalmazást. Most a zöld progress bar akkor kezd el növekedni, amikor a piros már készen van.

## 11. feladat: cancel funkcionalitás

Gyakran előfordul, hogy a felhasználó szeretné megállítani a háttérfolyamatokat. Olvasd el a
https://docs.microsoft.com/en-us/dotnet/standard/threading/cancellation-in-managed-threads
oldal elejét a "Code Example" résszel bezárólag!

Jelen esetben a ``Start_Click`` fogja létrehozni a CancellationTokenSource-t, amit egy osztály szintű attribútumban kell eltárolni, mivel a ``Cancel_Click`` eseménykezelő fog majd ennek szólni, hogy szakítsa meg a háttérfolyamatot.

Ahhoz, hogy a DoItAsync tudja figyelni a CancellationToken-t, át kell neki adnunk paraméterként a CancellationTokenSource.Token értékét.

Csak a piros progress bar legyen megszakítható, vagyis csak az első task indításnál adjuk át a CancellationTokent. Elegáns megoldás, ha a DoItAsync új CancellationToken paraméterének adunk default értéket, így a zöld progress bar eseténél nem kell megadni. Az alapértelmezett érték itt nem lehet null, mert a CancellationToken nem referencia, hanem érték típusú. De default értékűt kérhetünk a ``default(CancellationToken)`` kulcsszóval.

``public async Task DoItAsync(IProgress<int> progress, CancellationToken token = default(CancellationToken))``

A CancellationToken alapvetően csak jelezni tud, a DoItAsync-nek kell figyelnie rá és reagálni, vagyis a token semmire nem kötelezi a háttérszálakat.
A belső ciklusban a Task.Delay hívása után vizsgáld meg, hogy a token kér-e cancelt-t. Ha igen, akkor a háttérszálat egy OperationCanceledException dobásával szokás leállítani, ami konstruktor paraméterként megkapja a CancellationToken-t is.

Figyelj a fenti leírásban lévő "Cancellation in Managed Threads", fontosnak jelölt részekre. Valamit még meg kell tenni a CancellationTokennel, mielőtt véget ér a program. De csak akkor, miután már biztos nem használjuk. Hol érdemes ezt megtenni? Gondolj arra, hogy a zöld progress bar is csak akkor kezd el haladni, amikor a piros már véget ért.




