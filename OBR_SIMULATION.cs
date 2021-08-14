// area de resgate da D4RKMODE

string ultimoVerde = "";
string statusVerde = "";
string lastAction = "right";
string status = "";
string direcaoAtual = "";
string direcaoCaixa = "";
string direcaoSaida = "";


// carro preto
int ultraEsquerda = 0;
int ultraDireita = 2-1;
int ultraFrenteCima = 1-1;
int ultraFrenteBaixo = 3-1;

int error = 0;
int lastError = 0;
float basespeed = 150; //150
float initialBasespeed = 0;
float valInclinacao = bc.Inclination();
int i = 0;
float c = 0;
float valDirecao = bc.Compass();
float tempoInicial = bc.Millis();
float tempoAtual = bc.Millis();
float deltaTempo = 0;
int anguloInicial = 0;
int anguloCaixaDireita = -1;
int anguloCaixaEsquerda = -1;
float comprimentoDoCarro = 0;
float ladoArena = 97;
int saidaResgate = -1;
int update_time = 75; // taxa de atualização dos sensores (tem que ser int)
float constante = 10;

float Diagonal = 0;
float d = 0;
int a = 0;
int x = 0;
float y = 0;
int z = 0;
float r = 0;



float Kp = 500; // 500 
float Ki = 0.5f;//1
float Kd = 50; //1
float P = 0;
float I = 0;
float D = 0;
float speedR = 0;
float speedL = 0;
float max_speed = 200;
float pid_speed = 0;

bool gap = false;
bool estaRampa = false;
bool cruzamento = false;
bool possivelCruzamento = false;
bool vitimaAFrente = false;
bool rampaResgate = false;
bool condicao = false;
bool estavaRampa = false;
bool temVerde = false;
bool object_ahead = false;

void Main()
{
    initialBasespeed = basespeed;
    bc.SetPrecision(2);
    r = (float)39.68;
    comprimentoDoCarro = (r/2);
    ladoArena = ladoArena + comprimentoDoCarro;

    bc.ActuatorSpeed(150);
    levantarAtuador();
    girarbaixoBalde();

    calcularErro();
    if (gap) { bc.MoveFrontalAngles(500, 1); }
    while (gap) {
        bc.MoveFrontal(-300, -300);
        bc.Wait(update_time);
        calcularErro();
    }
    stop();

    while (!rampaResgate) {
        MainProcess();
        //bc.Wait(update_time);
    }
    bc.ClearConsole();
    bc.PrintConsole(0, "ENTROU NA AREA DE RESGATE");
    stop();
    RescueProcess();

    while (true) {
        bc.ClearConsole();
        bc.PrintConsole(0, "-=-=-=-=-=-=-= FIM -=-=-=-=-=-=-=");
        bc.PrintConsole(1, "    foram resgatas x vitimas    ");
        bc.Wait(10*1000);
    }
   
}

void evitarChoque() {
    bc.MoveFrontal(200, 200);
    bc.Wait(500);
    stop();
    bc.MoveFrontal(-200, -200);
    bc.Wait(500);
    stop();
}

void girarcimaBalde() {
    valDirecao = (float)Math.Truncate(valDirecao);

    int scoop_current = (int)Math.Truncate((float)bc.AngleScoop());
    while (scoop_current!=0 && scoop_current!=358 && scoop_current!=357) {
        scoop_current = (int)Math.Truncate((float)bc.AngleScoop());
        bc.TurnActuatorUp(1);
    }
}

void girarbaixoBalde() {
    int scoop_current = (int)Math.Truncate((float)bc.AngleScoop());
    while (scoop_current!=12 && scoop_current!=10) {
        scoop_current = (int)Math.Truncate((float)bc.AngleScoop());
        bc.TurnActuatorDown(1);
    }
}

void levantarAtuador() {
    int actuator_current = (int)Math.Truncate((float)bc.AngleActuator());
    while (actuator_current!=89 && actuator_current!=88) {
        actuator_current = (int)Math.Truncate((float)bc.AngleActuator());
        bc.ActuatorUp(1);
    }
}

void baixarAtuador() {
    int actuator_current = (int)Math.Truncate((float)bc.AngleActuator());
    while (actuator_current!=0 && actuator_current!=359) {
        actuator_current = (int)Math.Truncate((float)bc.AngleActuator());
        bc.ActuatorDown(1);
    }
}

void soltarVitimas() {
    bc.ActuatorSpeed(150);
    baixarAtuador();
    girarcimaBalde();

    
    bc.OpenActuator();
    bc.Wait(500);
    bc.CloseActuator();
    bc.Wait(500);

    /*
    bc.OpenActuator();
    while (bc.HasVictim()) {}
    bc.CloseActuator();
    */

    // levantar garra denovo
    bc.ActuatorSpeed(150);
    levantarAtuador();
    girarbaixoBalde();
}

void levantarGarra() {
    //bc.ActuatorSpeed(60);
    bc.ActuatorSpeed(150);
    bc.CloseActuator();
    //bc.Wait(1000);
    //bc.ActuatorSpeed(40);
    bc.ActuatorSpeed(150);
    levantarAtuador();
    girarbaixoBalde();
}

void abaixarGarra() {
    bc.ActuatorSpeed(150);
    girarcimaBalde();
    baixarAtuador();
    bc.OpenActuator();
    bc.Wait(100);
}

void percorrerDistancia(float distanciaDesejada) {
    float distancia = 0;
    bc.ResetTimer();

    while (distancia < Math.Abs(distanciaDesejada)) {
        if (distanciaDesejada > 0) { bc.MoveFrontal(basespeed, basespeed); } else { bc.MoveFrontal(-basespeed, -basespeed); }
        distancia = (basespeed*bc.Timer())/10000;
    }
    stop();
}

// ver o erro quando salvar bem aqui
void calcularDirecaoAtual() {
    valDirecao = (float)bc.Compass();
    if (valDirecao>=330 || valDirecao<=30) {direcaoAtual="norte";}
	if (valDirecao>=150 && valDirecao<=210) {direcaoAtual="sul";}
	if (valDirecao>=60 && valDirecao<=120) {direcaoAtual="leste";}
	if (valDirecao>=240 && valDirecao<=300) {direcaoAtual="oeste";}

}

void calcularErro() {
    object_ahead = bc.Distance(ultraFrenteBaixo)<=12 && !estaRampa && !rampaResgate;
    status = bc.ReturnColor(4) + " " + bc.ReturnColor(3) + " " + bc.ReturnColor(2) + " " + bc.ReturnColor(1) + " " + bc.ReturnColor(0);
    status = status.Replace("BRANCO", "0");
    status = status.Replace("AMARELO", "0");
    status = status.Replace("VERDE", "0");
    status = status.Replace("VERMELHO", "0");
    status = status.Replace("PRETO", "1");

    //bc.PrintConsole(2, status);

    // 1 verde 0 outras cores
    statusVerde = "";
    if ((bc.ReturnColor(4)=="VERDE" || bc.ReturnColor(3)=="VERDE") && (bc.ReturnColor(1)=="VERDE" || bc.ReturnColor(0)=="VERDE")) {statusVerde="1 1";}

    if ((bc.ReturnColor(4)=="VERDE" || bc.ReturnColor(3)=="VERDE") && (bc.ReturnColor(1)!="VERDE" && bc.ReturnColor(0)!="VERDE")) {statusVerde="1 0";}

    if ((bc.ReturnColor(4)!="VERDE" && bc.ReturnColor(3)!="VERDE") && (bc.ReturnColor(1)=="VERDE" || bc.ReturnColor(0)=="VERDE")) {statusVerde="0 1";}

    temVerde = statusVerde!="0 0" && statusVerde!="";

    gap = false;
    cruzamento = false;
    possivelCruzamento = false;

    switch (status) {
		case "0 1 1 1 1":
			error = 5;
			possivelCruzamento = true;
            break;
		case "0 0 1 1 1":
			error = 5;
			possivelCruzamento = true;
            break;
		case "0 0 0 1 1": error = 3; break;
		case "0 1 0 1 1": error = 4; break;
		case "0 0 0 1 0": error = 2; break;
		case "0 0 1 1 0":
			error = 2;
			//bc.MoveFrontal(basespeed, basespeed);
			//bc.Wait(100);
			//stop();
            break;
		case "0 0 1 0 0": error = 0; break;
		case "0 1 1 1 0": error = 0; break;
        case "1 1 1 1 1": error = 0; possivelCruzamento = true; break;

        // possiveis 90 graus
        case "0 0 1 0 1": error = 1; possivelCruzamento = true; break;
        case "0 1 0 0 1": error = 1; possivelCruzamento = true; break;
        case "1 0 1 0 0": error = -1; possivelCruzamento = true; break;
        case "1 0 0 1 0": error = -1; possivelCruzamento = true; break;

		
		case "0 1 1 0 0":
            error = -2;
			//bc.MoveFrontal(basespeed, basespeed);
			//bc.Wait(100);
			//stop();
            break;
		case "0 1 0 0 0": error = -2; break;
		case "1 1 0 0 0": error = -3; break;
		
		
        case "0 0 0 0 1": error = 5; break;
        case "1 0 0 0 0": error = -5; break;

		case "1 1 0 1 0": error = -4; break;
		case "1 1 1 0 0":
			error = -5;
			possivelCruzamento = true;
            break;
		case "1 1 1 1 0": 
			possivelCruzamento = true;
			error = -5;
            break;
		case "0 0 0 0 0": gap = true; break;
		default:
            break;
	}

    if (error > 0) {lastAction = "right";}
    if (error < 0) {lastAction = "left";}

    if (temVerde) {
        possivelCruzamento = false;
        cruzamento = false;
    }

    valDirecao = bc.Compass();
    calcularDirecaoAtual();

    valInclinacao = bc.Inclination();
    valInclinacao = (float)Math.Truncate(valInclinacao);
    //estaRampa = valInclinacao>5 && valInclinacao<356;
    estaRampa = valInclinacao>21 && valInclinacao<350;

    //bc.PrintConsole(1, status);
}

void calcularDeltaTempo() {
    tempoAtual = bc.Millis();
    deltaTempo = (tempoAtual - tempoInicial)/1000;
}

void passarCruzamento() {
    percorrerDistancia((float)11.25);
}

void alinharNaDirecaoAtual() {
    calcularDirecaoAtual();
    // alinha com a rampa
	// x -> angulo certinho da direcao
	// y -> negativo(1) esquerda e 1 direita
    float direction_current = (float)bc.Compass();

    if (direcaoAtual=="norte" && direction_current<30) {
		// esquerda
		x = 0;
		y = -1;
	} else if (direcaoAtual=="norte" && direction_current>30) {
		// direita
		x = 0;
		y = 1;
	} else if (direcaoAtual=="leste" && direction_current>90 && direction_current<90+30) {
		// esquerda
		x = 90;
		y = -1;
	} else if (direcaoAtual=="leste" && direction_current>90-30 && direction_current<90) {
		// direita
		x = 90;
		y = 1;
	} else if (direcaoAtual=="sul" && direction_current>180 && direction_current<180+30) {
		// esquerda
		x = 180;
		y = -1;
	} else if (direcaoAtual=="sul" && direction_current>180-30 && direction_current<180) {
		// direita
		x = 180;
		y = 1;
	} else if (direcaoAtual=="oeste" && direction_current>270 && direction_current<270+30) {
		// esquerda
		x = 270;
		y = -1;
	} else if (direcaoAtual=="oeste" && direction_current>270-30 && direction_current<270) {
		// direita
		x = 270;
		y = 1;
	} else {}

    while (Math.Truncate(bc.Compass())!=x) {
        if (y==1) {
            bc.MoveFrontalAngles(300, (float)0.3);
        } else {
            bc.MoveFrontalAngles(300, -(float)0.3);
        }

        if (x==0 && (int)Math.Truncate(bc.Compass())==359) {break;}
    }
}

void varrerLado() {
    // vai até a parede pega as vitimas e volta pra posicao inicial
    float initial_ultra = bc.Distance(ultraFrenteCima);
    while (bc.Distance(ultraFrenteCima)>30) {
        bc.MoveFrontal(basespeed, basespeed);
    }
    stop();

    alinharNaDirecaoAtual();
    levantarGarra();
    //while (bc.Distance(ultraFrenteBaixo)>2) {bc.MoveFrontal(basespeed, basespeed);}
    //stop();
    
    while (bc.Distance(ultraFrenteCima)<initial_ultra) {
        bc.MoveFrontal(-basespeed, -basespeed);
    }
    
    stop();
}

void GoCaixa() {
    if (bc.Distance(ultraFrenteBaixo)<31) { evitarChoque(); }
    abaixarGarra();
    percorrerDistancia((float)((ladoArena/2)*1.2));
    levantarGarra();
    while (bc.Distance(ultraFrenteBaixo)>=1) { bc.MoveFrontal(300, 300); }
    stop();
}

void desviarVerde() {
    bc.MoveFrontalRotations(300, (float)0.3); stop();
    calcularErro();
    ultimoVerde = statusVerde;

    bc.TurnLedOn(0, 150, 0);

    
    string ordemVerde = "";
    int coeficiente = 1;

    //if (ultimoVerde=="0 1" && statusVerde=="0 0") {
    bc.PrintConsole(2, ultimoVerde);
    if (ultimoVerde=="1 1") {
        ordemVerde = "spin";
        stop();
    }

    if (ultimoVerde=="0 1") {
        ordemVerde = "right";
        coeficiente = 1;
    }
    //if (ultimoVerde=="1 0" && statusVerde=="0 0") {
    if (ultimoVerde=="1 0") {
        ordemVerde = "left";
        coeficiente = -1;
    }

    bc.PrintConsole(1, "Passou.");
    bc.PrintConsole(2, ordemVerde);

    //int rotations_green = 13;
    int rotations_green = 5;

    if (ordemVerde=="spin") {
        bc.MoveFrontalAngles(1000, 180);
        //bc.MoveFrontalRotations(-300, 1);
    } else {
        bc.MoveFrontalRotations(300, rotations_green);
        stop();
        calcularErro();
        if (!gap) {
            if (((lastError>0 && ordemVerde=="right") || (lastError<0 && ordemVerde=="left")) && bc.ReturnColor(2)!="PRETO") {
                while (bc.ReturnColor(2)!="PRETO") {
                    if (lastAction=="right") {
                        bc.MoveFrontalAngles(1000, 1);
                    } else {
                        bc.MoveFrontalAngles(1000, -1);
                    }
                }
            }
            
        }

        while (bc.ReturnColor(2)!="BRANCO") {
            bc.MoveFrontalAngles(1000, coeficiente);
        }
        
        while (bc.ReturnColor(2)!="PRETO") {
            bc.MoveFrontalAngles(1000, coeficiente);
        }

    }
    stop();
    bc.ClearConsole();
    ultimoVerde = "0 0";
    calcularErro();
}

bool desviarObstaculo() {
    // desviar de obstaculo
    calcularErro();
    if (object_ahead) {
        bc.ClearConsole();
        stop();
        bc.PrintConsole(0, "avistei obstaculo");
        alinharNaDirecaoAtual();

        bc.PrintConsole(1, "desviando...");

        // distancia aceitavel 13
        while (bc.Distance(ultraFrenteBaixo)<=13) {
            bc.MoveFrontal(-basespeed, -basespeed);
        }


        bc.MoveFrontalAngles(500, 12);

        
        // vira 45 graus pra direita
        while (true) {
            bc.MoveFrontalAngles(500, 1);
            c = (int)Math.Truncate(bc.Compass())%45;
            if ((c>=43 && c<=44) || c==0) {break;}
        }
        
        c = 1;
        // se tiver algo que pare o carrinho de um lado (ex: rampa)
        // girar para o outro
        if (bc.Distance(ultraFrenteBaixo)<45) {
            bc.PrintConsole(2, "objeto na direita, girando sentido contrario...");
            bc.MoveFrontalAngles(1000, -90);
            c = -1;
        }


        
        bc.MoveFrontalRotations(300, 22);
        bc.MoveFrontalAngles(1000, -45*c);
        alinharNaDirecaoAtual();
        bc.MoveFrontalRotations(300, 15);
        bc.MoveFrontalAngles(1000, -45*c);
        bc.MoveFrontalRotations(300, 22);
        bc.MoveFrontalAngles(1000, 45*c);
        alinharNaDirecaoAtual();

        calcularErro();
        if (gap) {
            for (i=0; i<=5; i++) {
                bc.MoveFrontalRotations(300, 1);
                calcularErro();
                if (!gap) {
                    bc.MoveFrontalAngles(1000, error);
                    return true;
                }
            }
        }
        return true;
    }
    return false;
}

void MainProcess() {
    I = 0;
    estavaRampa = false;
    //bc.Wait(update_time);
    lastError = error;
    calcularErro();
    calcularDeltaTempo();
    bc.TurnLedOn(255, 255, 255);
    basespeed = initialBasespeed;
    estaRampa = false;

    if (lastError>0) {lastAction="right";}
    if (lastError<0) {lastAction="left";}

    //if (!cruzamento && !possivelCruzamento && !temVerde && !gap) {
    
    bc.ClearConsole();
    int repetitions = 0;
    //while (!cruzamento && !possivelCruzamento && !temVerde && !gap) {
    float last_speedR = 0;
    float last_speedL = 0;
    bc.ResetTimer();
    while (!possivelCruzamento && !temVerde && !object_ahead && !gap) {
        // correcao
        calcularErro();
        bc.PrintConsole(0, "error="+error.ToString());
        // bc.PrintConsole(1, "lastError="+lastError.ToString());
        if (error==0) {
            bc.MoveFrontal(basespeed, basespeed);
        } else {
            for (int x=1; x<=Math.Abs(error); i++) {
                bc.PrintConsole(1, "procurando linha");
                if (error>0) {
                    bc.MoveFrontalAngles(1000, 1);
                } else {
                    bc.MoveFrontalAngles(1000, -1);
                }
                if (bc.ReturnColor(2)=="PRETO") { break; }
            }
            //bc.MoveFrontalAngles(1000, error);
        }
        if ((error>0 && lastError<0) || (error<0 && lastError>0)) {
            bc.MoveFrontalRotations(300, 1);
        }
        lastError = error;
        calcularErro();
    }
    lastError = error;
    stop();

    if (temVerde) { // SE TIVER VERDE
        bc.ClearConsole();
        bc.PrintConsole(0, "CAIU NO IF DO VERDE");
        desviarVerde();
        return;
    }

    bool green_appeared = false; // se pelo menos 1 vez apareceu o verde vai ser verdadeiro
    if (possivelCruzamento) {
        //stop();
        bc.ClearConsole();
        bc.PrintConsole(0, "possivel cruzamento");
        //float closed_curve_rotations = 4; //4 ideal
        
        //vai x rotacoes pra frente pra ver se é a curva fechada
        //bc.MoveFrontalRotations(300, 14);
        bc.MoveFrontalRotations(300, 5);
        stop();

        bc.Wait(update_time);
        calcularErro();
        // se for a curva fechada girar pro sentido contrario do ultimo erro 
        if (status=="1 1 1 1 1") {
            bc.PrintConsole(1, "caso especial curva fechada, girando...");

            bc.MoveFrontalRotations(300, 7);
            // girar pro sentido contrario do ultimo possivel cruzamento
            if (lastError<=0) {
                // girar 90 graus pra direita
                girar90Graus("right");
            } else {
                // girar 90 graus pra esquerda
                girar90Graus("left");
            }
        
        } else { // casos normais
            bc.MoveFrontalRotations(300, -2);
            //stop();
            bc.Wait(update_time);
            calcularErro();
            if (gap) { // não é cruzamento então tem que fazer a curva de 90 graus
                // 90 GRAUS
                bc.PrintConsole(1, "entrando no 90 graus");
                if (lastError<0) {
                    girar90Graus("left");
                } else {
                    girar90Graus("right");
                }
            } else {
                bc.PrintConsole(1, "cruzamento identificado");
                stop();
                passarCruzamento();
            }

        }
        return;
    }


    // se tem obstaculo -> desvia e retorna true
    // senao -> nao faz nada e retorna false
    bool temObstaculo = desviarObstaculo();
    if (temObstaculo) {return;}

    //if (estaRampa && bc.Distance(ultraFrenteBaixo)>30) {
    if (estaRampa) {
        basespeed = 100;
        calcularDirecaoAtual();
        bc.PrintConsole(0, "esta em uma rampa a " + direcaoAtual);
        bc.MoveFrontal(basespeed, basespeed);

        percorrerDistancia(12);
        alinharNaDirecaoAtual();

        //while (estaRampa && bc.Distance(ultraFrenteBaixo)>30) {
        while (estaRampa) {
            bc.Wait(update_time);
            rampaResgate = bc.Distance(ultraDireita)<=38 || rampaResgate;
            if (rampaResgate) {bc.PrintConsole(0, "ENTRANDO NA RAMPA DE RESGATE");}

            calcularErro();
            if (error==0 || gap) {
                if (rampaResgate) {basespeed = 200;}
                bc.MoveFrontal(basespeed, basespeed);
            } else {
                bc.MoveFrontalAngles(500, error);
            }
        }

        estavaRampa = true;
        bc.ClearConsole();
        bc.PrintConsole(0, "saindo da rampa");
        return;
    }

    if (gap && !estaRampa) { // gap
        // usar 16 rotacoes
        stop();
        bc.ClearConsole();
        bc.PrintConsole(0, "gap identificado");
        int degrees_verification = 2;
        if (lastAction=="right") {
            bc.MoveFrontalAngles(500, degrees_verification);
        } else  {
            bc.MoveFrontalAngles(500, -degrees_verification);
        }

        bc.Wait(update_time);
        calcularErro();
        if (!gap) {
            return;
        }

        int gap_size = 14; // 14 ou 15
        for (i=1; i<=gap_size; i++) {
            bc.MoveFrontalRotations(300, 1);
            calcularErro();
            if (status=="1 1 1 1 1") {
                bc.ClearConsole();
                bc.PrintConsole(0, "caso especial curva fechada depois do gap");
                bc.MoveFrontalRotations(300, 10);
                alinharNaDirecaoAtual();

                calcularDirecaoAtual();
                
                if (direcaoAtual=="leste") {
                    alinharNaDirecaoAtual();
                    return;
                }

                string last_direction = direcaoAtual;
                gap = true;
                while (gap || direcaoAtual!="leste") {
                    calcularErro();
                    calcularDirecaoAtual();
                    // dar um jeito pra descobrir pra qual lado a pista continua
                    if (last_direction=="norte" || last_direction=="oeste") {
                        bc.MoveFrontalAngles(500, 1);
                        bc.PrintConsole(1, "girando pra direita");
                    } else { // se for sul
                        bc.MoveFrontalAngles(500, -1);
                        bc.PrintConsole(1, "girando pra esquerda");
                    }
                    //bc.Wait(update_time);
                }
                return;
            }

            if (!gap) {return;}
        }

        stop();

        if (lastError!=0) {
            bc.MoveFrontalRotations(-300, gap_size/2);
            while (gap) {
                bc.MoveFrontalAngles(1000, lastError);
                calcularErro();
            }
        } else {
            //gap = false;
            //while (!gap) {
            //    bc.MoveFrontal(-300, -300);
            //    calcularErro();
            //}

            calcularErro();
            while (gap) {
                bc.MoveFrontal(-300, -300);
                calcularErro();
            }

            stop();
            
            if (lastAction=="right") {
                bc.MoveFrontalAngles(1000, -1);
            } else {
                bc.MoveFrontalAngles(1000, 1);
            }
        }
        //stop();
        return;
    }
}

void buscarCaixa() {
    alinharNaDirecaoAtual();
    // falta programar:
    // direcaoSaida=="left"
    // direcaoSaida=="up"
    

    if (direcaoSaida=="right") {
        bc.MoveFrontalRotations(300, 72);
        stop();
        levantarGarra();

        
        bc.MoveFrontalRotations(300, 15);
        stop();

        bc.Wait(20);
        if (bc.Distance(ultraFrenteBaixo)<12) {
            direcaoCaixa = "up_left";
            bc.ClearConsole();
            bc.PrintConsole(0, "achou caixa: "+direcaoCaixa);

            // vai alinhar e ficar de frente pra caixa
            bc.MoveFrontalAngles(1000, 5);
            goToNextDivisible(45, "right");

            bc.MoveFrontalRotations(300, 14);

            bc.MoveFrontalAngles(1000, -89); // 90 graus fica um pouquinho torto
        } else {
            direcaoCaixa = "up_right";
            bc.ClearConsole();
            bc.PrintConsole(0, "achou caixa: "+direcaoCaixa);

            encostarNaParede();


            bc.MoveFrontalAngles(1000, 90);
            alinharNaDirecaoAtual();

            
            while (bc.Distance(ultraFrenteCima)>92) {
                bc.MoveFrontal(basespeed, basespeed);
            }
            stop();

            // vai alinhar e ficar de frente pra caixa
            bc.MoveFrontalAngles(1000, 5);
            goToNextDivisible(45, "right");

            bc.MoveFrontalRotations(300, 28);

            bc.MoveFrontalAngles(1000, -89); // 90 graus fica um pouquinho torto
        }
        return;
    }

    if (direcaoSaida=="left") {
        alinharNaDirecaoAtual();
        
        while (bc.Distance(ultraFrenteCima)>128) { bc.MoveFrontal(basespeed, basespeed); }
        stop();

        levantarGarra();
        bc.MoveFrontalRotations(300, 15);
        stop();

        bc.Wait(20);
        if (bc.Distance(ultraFrenteBaixo)<12) {
            direcaoCaixa = "down_right";
            bc.ClearConsole();
            bc.PrintConsole(0, "achou caixa: "+direcaoCaixa);

            // vai alinhar e ficar de frente pra caixa
            bc.MoveFrontalAngles(1000, -5);
            goToNextDivisible(45, "left");

            bc.MoveFrontalRotations(300, 33);

            bc.MoveFrontalAngles(1000, 89); // 90 graus fica um pouquinho torto
        } else {
            direcaoCaixa = "up_right";
            bc.ClearConsole();
            bc.PrintConsole(0, "achou caixa: "+direcaoCaixa);

            encostarNaParede();

            bc.MoveFrontalAngles(1000, -90);
            alinharNaDirecaoAtual();

            while (bc.Distance(ultraFrenteCima)>92) { bc.MoveFrontal(basespeed, basespeed); }
            stop();

            // vai alinhar e ficar de frente pra caixa
            bc.MoveFrontalAngles(1000, -5);
            goToNextDivisible(45, "left");

            bc.MoveFrontalRotations(300, 28);

            bc.MoveFrontalAngles(1000, 89); // 90 graus fica um pouquinho torto
        }
        return;
    }

    if (direcaoSaida=="up") {
        bc.MoveFrontalRotations(300, 72);
        stop();
        levantarGarra();

        bc.MoveFrontalRotations(300, 15);
        stop();

        bc.Wait(20);
        if (bc.Distance(ultraFrenteBaixo)<12) {
            direcaoCaixa = "up_left";
            bc.ClearConsole();
            bc.PrintConsole(0, "achou caixa: "+direcaoCaixa);

            // vai alinhar e ficar de frente pra caixa
            bc.MoveFrontalAngles(1000, 5);
            goToNextDivisible(45, "right");

            bc.MoveFrontalRotations(300, 16);

            bc.MoveFrontalAngles(1000, -89); // 90 graus fica um pouquinho torto
        } else {
            direcaoCaixa = "down_right";
            bc.ClearConsole();
            bc.PrintConsole(0, "achou caixa: "+direcaoCaixa);
            encostarNaParede();

            bc.MoveFrontalAngles(1000, 90);
            alinharNaDirecaoAtual();
            
            encostarNaParede();

            bc.MoveFrontalAngles(1000, 90);
            alinharNaDirecaoAtual();
            
            while (bc.Distance(ultraFrenteCima)>92) {
                bc.MoveFrontal(basespeed, basespeed);
            }
            stop();

            // vai alinhar e ficar de frente pra caixa
            bc.MoveFrontalAngles(1000, 5);
            goToNextDivisible(45, "right");

            bc.MoveFrontalRotations(300, 28);

            bc.MoveFrontalAngles(1000, -89); // 90 graus fica um pouquinho torto
        }
        return;
    }

}

void RescueProcess() {
    // vai um pouquinho pra frente pra entrar realmente na área
    alinharNaDirecaoAtual();
    bc.MoveFrontalRotations(300, 10);
    stop();

    while (bc.Inclination()!=0) {
        bc.MoveFrontal(50, 50);
    }

    stop();

    alinharNaDirecaoAtual();
    bc.ClearConsole();

    i = 0;
    c = 0;
    basespeed = 300;
    initialBasespeed = basespeed;
    anguloInicial = (int)Math.Truncate(bc.Compass());
    bc.PrintConsole(0, "anguloInicial="+anguloInicial.ToString());


    if (bc.Distance(ultraFrenteBaixo)<31) { evitarChoque(); }

    bc.MoveFrontalAngles(1000, 35);
    if (bc.Distance(ultraFrenteBaixo)<31) { evitarChoque(); }

    if (bc.Distance(ultraFrenteCima)==10000) {
        if (bc.Distance(ultraFrenteBaixo)<31) { evitarChoque(); }
        saidaResgate = anguloInicial;
        direcaoSaida = "up";
        bc.MoveFrontalAngles(1000, -35);
    } else {
        if (bc.Distance(ultraFrenteBaixo)<31) { evitarChoque(); }
        bc.MoveFrontalAngles(1000, 55);
        if (bc.Distance(ultraFrenteBaixo)<31) { evitarChoque(); }
        if (bc.Distance(ultraFrenteCima)==10000) {
            saidaResgate = anguloInicial + 90;
            direcaoSaida = "right";
            bc.MoveFrontalAngles(1000, -90);
        } else {
            saidaResgate = anguloInicial - 90;
            direcaoSaida = "left";
        }
    }

    if (saidaResgate>=360) {saidaResgate=saidaResgate-360;}
    if (saidaResgate<0) {saidaResgate=360-saidaResgate;}
    bc.PrintConsole(1, "direcao da saida=" + direcaoSaida);
    alinharNaDirecaoAtual();

    abaixarGarra();

    // diagonal da arena
    Diagonal = (float)(ladoArena*(Math.Pow(2, 1/2))*1.2); // * 1.2 pra ter mais precisão 
    y = ladoArena/3; //nao pode apagar

    stop();

    ///////////////////////////////
    basespeed = 300;
    bc.ClearConsole();
    bc.PrintConsole(0, "<color=\"red\">iniciando resgate das vitimas</color>");
    //////// PARTE PRINCIPAL
    /////////////////////////////////////////////////////
    
    float initial_direction = (float)Math.Truncate(bc.Compass());
    bool rescued_all_victims = false;
    float rescued_victims = 0;
    float ultraR;
    float ultraU;
    float ultraD;

    buscarCaixa();

    while (bc.Distance(ultraFrenteBaixo)>1.5f) { bc.MoveFrontal(basespeed, basespeed); }
    if (bc.HasVictim()) { soltarVitimas(); }

    // usar aqui o método de resgate
    // nesse caso vou usar algo parecido com a lógica da darkmode

    int box_coordinate = (int)Math.Truncate(bc.Compass());
    bc.PrintConsole(1, "box_coordinate="+box_coordinate.ToString());
    
    while (true) {
        bc.MoveFrontalRotations(-basespeed, 55);
        stop();
        
        for (int i=0; i<=225; i++) {
            // vai girando pra direita
            bc.MoveFrontalAngles(1000, 1);
            // condicao: <58 ou <78 ou < 106 ou < 108
            bool achou_bolinha = bc.Distance(ultraFrenteBaixo)<108;
            if (achou_bolinha) {
                if (bc.Distance(ultraFrenteBaixo)<31) { evitarChoque(); }
                break;
            }
        }

        abaixarGarra();
        bc.ResetTimer();
        // vai até pegar a bolinha na garra
        while (!bc.HasVictim()) { bc.MoveFrontal(basespeed, basespeed); }
        int back_time = bc.Timer();
        stop();
        levantarGarra();

        // volta pro centro
        bc.MoveFrontal(-basespeed, -basespeed);
        bc.Wait(back_time);
        stop();

        // gira pra caixa denovo
        goToDirection(box_coordinate, "left");
        
        //if (bc.Distance(ultraFrenteBaixo)<31) { evitarChoque(); }
        //abaixarGarra();

        // encosta na caixa
        while (bc.Distance(ultraFrenteBaixo)>2) { bc.MoveFrontal(basespeed, basespeed); }
        stop();

        if (bc.HasVictim()) { soltarVitimas(); rescued_victims += 1;}
    }
}

void goToNextDivisible(int divider, string action) {
    while (true) {
        if (action=="left") {
            bc.MoveFrontalAngles(500, -1);
        } else {
            bc.MoveFrontalAngles(500, 1);
        }
        
        c = (int)Math.Truncate(bc.Compass())%divider;
        if ((c>=(divider-2) && c<=(divider-1)) || c==0) {break;}
    }
}


void goToDirection(int coordinate, string action) {
    while (true) {
        if (action=="left") {
            bc.MoveFrontalAngles(400, -0.1f);
        } else {
            bc.MoveFrontalAngles(400, 0.1f);
        }
        
        if (Math.Truncate(bc.Compass())==coordinate) { break; }
    }
}


void girar90Graus(string direction="right") {
    bc.ClearConsole();
    bc.PrintConsole(0, "entrando no 90 graus");
    for (i=1; i<=90; i++) {
        if (direction=="right") {
            bc.MoveFrontalAngles(1000, 1);
        } else if (direction=="left") {
            bc.MoveFrontalAngles(1000, -1);
        } else {
            //bc.MoveFrontal(basespeed, basespeed);
            bc.MoveFrontalAngles(500, 1); // pra direita pq sempre vai ser mt provavel ser ao leste
        }
        calcularErro();
        if (!gap) {
            stop();
            return;
        }
        bc.PrintConsole(1, "girou " + i + " graus");
    }
    stop();
    if (direction=="left") {
        bc.MoveFrontalAngles(1000, 90);
    } else {
        bc.MoveFrontalAngles(1000, -90);
    }

    while (bc.ReturnColor(2)!="PRETO") {
        bc.MoveFrontal(-basespeed, -basespeed);
    }
    stop();
    bc.MoveFrontalAngles(500, lastError);
}

void stop() {
    bc.MoveFrontal(0, 0);
}

void encostarNaParede() {
    if (bc.Distance(ultraFrenteBaixo)<=31) { evitarChoque(); }
    abaixarGarra();

    while (bc.Distance(ultraFrenteCima)>29) {
        bc.MoveFrontal(basespeed, basespeed);
    }
    stop();
    levantarGarra();

    while (bc.Distance(ultraFrenteBaixo)>2) {
        bc.MoveFrontal(basespeed, basespeed);
    }
    stop();
}