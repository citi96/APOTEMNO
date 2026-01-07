1. Stack Tecnologico & Setup Iniziale
Engine: Godot 4.3 (Stable). La versione 4 è necessaria per i nuovi shader di distorsione e per la gestione avanzata dei Viewport (fondamentale per la geometria non euclidea).

Stile Grafico: 1-Bit Dithered (Bianco e Nero/Rosso Sangue).

Perché: Meno asset da disegnare, più inquietudine. Il cervello riempie i buchi neri con la paura.

Implementazione: Useremo uno shader di post-processing su un CanvasLayer per forzare la palette e il dithering, indipendentemente dagli sprite usati.

2. Sprint 1: L'Architettura dell'Inganno (Core Systems)
Dobbiamo costruire le fondamenta su cui il gioco mentirà al giocatore.

Ticket A: Il Sistema di Input "Mutilabile"
Invece di un input manager standard, creeremo un "InputBroker".

Logica: Tutti i comandi passano da qui. Quando il giocatore sacrifica una parte del corpo (es. "Lobo Parietale"), il Broker inizia a introdurre latenza o a scambiare i tasti casualmente.

Godot Implementation:

Creare un Autoload InputManagerGlobal.

Variabile var corruption_level: float = 0.0.

Se il giocatore preme "Destra", c'è una probabilità del corruption_level che il personaggio vada a sinistra o inciampi (animazione di caduta).

Ticket B: L'Interfaccia Parassita (UI)
L'UI non deve essere figlia del Player, ma un'entità a sé stante che lo osserva.

Task: Creare una barra della salute che mente.

Godot Implementation:

ProgressBar standard collegata a real_health.

ProgressBar "Fake" visibile, collegata a un timer casuale (TimerNode).

Quando il giocatore subisce danno, la barra fake potrebbe aumentare per confonderlo, o rimanere piena finché il personaggio non muore improvvisamente.

3. Sprint 2: Atmosfera e Distorsione (Tech Art)
Qui è dove rendiamo il gioco visivamente "tossico".

Ticket C: Shader "Dither & Degradation"
Non useremo la trasparenza normale. Useremo il Dithering per simulare la dissoluzione della realtà.

Riferimento: Tecnica usata in Return of the Obra Dinn o nei giochi horror 1-bit.

Codice Shader (Concetto):

Applicare uno shader a tutto schermo (ColorRect in un CanvasLayer superiore).

Usare una matrice di Bayer 4x4 o 8x8 per decidere quali pixel spegnere in base alla luminosità.

Twist Malato: Legare la densità del dithering alla "Sanità Mentale". Meno sanità = più rumore visivo, finché lo schermo diventa quasi illeggibile.

Ticket D: Geometria Non-Euclidea (Il Cenotafio)
Il mondo deve sembrare infinito e ricorsivo.

Task: Creare stanze che si collegano in modi impossibili (es. uscire a destra e rientrare dalla stessa porta a destra).

Godot Implementation:

Usare i SubViewport per creare texture di altre stanze e proiettarle su Sprite2D che fungono da porte.

Quando il giocatore tocca il portale (Area2D), teletrasportarlo istantaneamente (global_position) nella stanza target, mantenendo la continuità visiva. Questo crea l'effetto "loop infinito" senza caricamenti.

4. Sprint 3: Audio Psicoacustico (Sound Design)
L'audio deve fare male fisicamente o disorientare.

Ticket E: Il Tono di Shepard (Loop Infinito)
Concetto: Una scala musicale che sembra scendere all'infinito ma non diventa mai più grave.

Godot Implementation:

Creare 3 AudioStreamPlayer.

Caricare lo stesso sample in loop su tutti e tre, ma a ottave diverse (Pitch Scale 0.5, 1.0, 2.0).

Modulare il volume (Volume Db) con un AnimationPlayer: mentre uno sale di volume, l'altro scende, creando l'illusione di una discesa eterna nelle viscere del Cenotafio.

Ticket F: Infrasuoni e Binaurale
Task: Inserire suoni a bassissima frequenza (sotto i 20Hz o al limite dell'udibile) nei momenti di tensione, mixati con l'audio binaurale dell'Intruso.

Godot Implementation: Usare l'Audio Bus Layout per separare la voce dell'Intruso e bypassare il riverbero ambientale, facendola suonare come se fosse "dentro le cuffie" (dry signal), mentre i suoni del mondo sono processati con riverbero spaziale.