informatio n
Article
Simulated Autonomous Driving Using Reinforcement Learning:
A Comparative Study on Unity’s ML-Agents Framework
YusefSavid1,RezaMahmoudi1 ,RytisMaskeliu¯nas2 andRobertasDamaševicˇius2,*
1 DepartmentofMultimediaEngineering,FacultyofInformatics,KaunasUniversityofTechnology,
51368Kaunas,Lithuania
2 CenterofExcellenceForest4.0,FacultyofInformatics,KaunasUniversityofTechnology,
51368Kaunas,Lithuania
* Correspondence:robertas.damasevicius@ktu.lt
Abstract:Advancementsinartificialintelligenceareleadingresearcherstofindusecasesthatwere
notasstraightforwardtosolveinthepast.Theusecaseofsimulatedautonomousdrivinghasbeen
knownasanotoriouslydifficulttasktoautomate,butadvancementsinthefieldofreinforcement
learninghavemadeitpossibletoreachsatisfactoryresults. Inthispaper, weexploretheuseof
theUnityML-Agentstoolkittotrainintelligentagentstonavigatearacingtrackinasimulated
environmentusingRLalgorithms. ThepapercomparestheperformanceofseveraldifferentRL
algorithmsandconfigurationsonthetaskoftrainingkartagentstosuccessfullytraversearacing
trackandidentifiesthemosteffectiveapproachfortrainingkartagentstonavigatearacingtrackand
avoidobstaclesinthattrack.Thebestresults,valuelossof0.0013andacumulativerewardof0.761,
wereyieldedusingtheProximalPolicyOptimizationalgorithm.Aftersuccessfullychoosingamodel
andalgorithmthatcantraversethetrackwithease,differentobjectswereaddedtothetrackand
anothermodel(whichusedbehavioralcloningasapre-trainingoption)wastrainedtoavoidsuch
obstacles.Theaforementionedmodelresultedinavaluelossof0.001andacumulativerewardof
0.068,provingthatbehavioralcloningcanhelpachievesatisfactoryresultswheretheingameagents
areabletoavoidobstaclesmoreefficientlyandcompletethetrackwithhuman-likeperformance,
allowingforadeploymentofintelligentagentsinracingsimulators.
Citation:Savid,Y.;Mahmoudi,R.;
Keywords:reinforcementlearning;autonomousdriving;virtualrobotics;simulation
Maskeliu¯nas,R.;Damaševicˇius,R.
SimulatedAutonomousDriving
UsingReinforcementLearning:A
ComparativeStudyonUnity’s
1. Introduction
ML-AgentsFramework.Information
2023,14,290. https://doi.org/ Reinforcementlearning(RL)isapowerfulapproachfortrainingintelligentagentsto
10.3390/info14050290 performawiderangeoftasks,fromplayinggamestonavigatingcomplexenvironments[1].
Thepossibleapplicationsincludethefollowing: robotics,whereRLcanbeusedtotrain
AcademicEditor:KatsuhideFujita
robotstoperformtaskssuchasgraspingobjectsornavigatingthroughaphysicalenvi-
Received:10April2023 ronment[2]withstrongpotentialtodriveadaptingindustrialenvironments,apotential
Revised:9May2023 technological driver to emerge under the revolution of Industry 4.0 [3]; game playing,
Accepted:12May2023 where RL has been used to train agents to play games [4] such as chess, Go and poker
Published:14May2023
atasuperhumanlevel;Autonomousvehicles,whereRLcanbeusedtotrainself-driving
carstomakedecisions[5]suchaswhentochangelanesorwhentostopatatrafficlight;
healthcare,whereRLcanbeusedtooptimizetreatmentplansforpatientsandtodesign
personalizedmedicine[6,7];industrialcontrol,whereRLcanbeusedtooptimizecontrolof
Copyright: © 2023 by the authors.
industrialsystems[8,9]suchaspowergrids,watertreatmentplantsandfactories;energy,
Licensee MDPI, Basel, Switzerland.
whereRLcanbeusedtooptimizeenergyconsumption[10],renewableenergysystems
This article is an open access article
andstorage;recommendationsystems,whereRLcanbeusedtooptimizepersonalized
distributed under the terms and
recommendationsforusers[11],suchasproductsormoviestowatch;cybersecurity,where
conditionsoftheCreativeCommons
Attribution(CCBY)license(https:// RLcanbeusedtotrainagentstodetectandrespondtocyberattacks[12].
creativecommons.org/licenses/by/ OneparticularlypromisingapplicationofRLisinthedevelopmentofautonomous
4.0/). racingagents[13,14],whichcannavigatearacingtrackandmakedecisionsabouthow
Information2023,14,290.https://doi.org/10.3390/info14050290 https://www.mdpi.com/journal/information

Information2023,14,290 2of22
tonavigateobstaclesandothercompetitorsinrealtime. Inmostcases, particularlyfor
testing,simulationenvironmentsareused[15,16]. Onekeyadvantageofusingsimulation
environmentsforthetrainingofRLagentsistheabilitytoeasilygeneratelargeamountsof
datafortrainingandtesting. Thisisparticularlyimportantfortaskssuchasautonomous
driving [17] and autonomous path planning [18,19] and tracking control [20,21], where
real-worlddatacanbedifficultandexpensivetocollect. Simulationenvironmentsalso
allowforgreatercontroloverthetrainingenvironment,enablingresearcherstoeasilyvary
factorssuchasthelayoutofthetrackortheweatherconditions[22].
Thedevelopmentofautonomousracingagentsisimportantformobileroboticsbe-
cause it presents a challenging problem that requires the integration of many different
skills,includingperception,control,pathplanning[23]anddecision-making. Autonomous
racinginvolvesnavigatingthroughadynamic,unstructuredenvironmentathighspeeds
while avoiding obstacles, moving over irregular terrain [24,25] and competing against
otheragents. Solvingthisproblemrequiresthedevelopmentofadvancedalgorithmsfor
perception,controlanddecision-making,aswellastheabilitytoprocesslargeamounts
ofdatainrealtime. Additionally,theuseofsimulationenvironmentsallowsforthesafe
andefficienttestingoftheseagentsincrowdedenvironments[19]orpartiallyunknown
environments[26],whichisimportantwhenworkingwithmobilerobots.Thedevelopment
ofautonomousracingagentscanalsoleadtothedevelopmentofmorecapablemobile
robotsthatcanbeusedinavarietyofdifferentapplications,suchassearchandrescue,
deliveryandothertasks. Thedevelopmentofautonomousracingagentscanalsoleadto
importantbreakthroughsinthefieldofrobotics,asthechallengesthatneedtobeovercome
for autonomous racing are similar to those that need to be overcome for other mobile
roboticapplications. Inaddition,autonomousracingcanserveasatestbedforvarious
technologies,suchassensors,cameras,lidarsandothertechnologiesthatcanbeusedin
mobilerobotics[27,28]. Thesetechnologiesarethenfurtherimprovedtomeetthedemands
ofautonomousracingandthencanbeusedinothermobileroboticapplications.
In this paper, we investigate the the Unity ML-Agents toolkit [29], a widely used
platformfortrainingintelligentagentsusingRLalgorithms,totrainkartagentstonavigate
aracingtrack. TheUnityengine[30]providesarichandrealisticsimulationenvironment
thatallowsforthedevelopmentandtestingofintelligentagentsinawiderangeofscenarios.
BytrainingkartagentstonavigatearacingtrackusingRLalgorithms,weaimtoidentify
themosteffectiveapproachfortrainingautonomousracingagents. Thenoveltyofthis
paperisthatitexplorestheuseoftheUnityML-Agentstoolkittotrainintelligentagentsto
navigatearacingtrackinasimulatedenvironmentusingRLalgorithms.
The paper contributes to the field by comparing the performance of several RL al-
gorithms, including Multi-Agent Proximal Policy Optimization (MA-PPO) and POCA
(ParallelOnlineContinuousArcing),intrainingintelligentagentstonavigatearacingtrack
inasimulatedenvironment. Additionally,thepaperproposestheuseofbehavioralcloning
asapre-trainingconditionwiththedefaultPPOalgorithmtoassistthemodelinlearning
therequiredbehaviorandtoimprovetheperformanceofintelligentagentsforsolvingthe
racingtask. Thepaperalsoprovidesinsightintothecapabilitiesandlimitationsofdifferent
RLalgorithmsandcaninformthedevelopmentofmoreeffectiveandefficientapproaches
fortrainingintelligentagentsinsimulatedenvironments. Ourresultsprovideinsightinto
therelativestrengthsandweaknessesofdifferentRLapproachesandcontributetothe
growingbodyofliteratureontheuseofRLalgorithmsfortrainingintelligentagentsin
simulatedenvironments[31,32].
2. StateoftheArtReview
Therehavebeennumerousadvancesinreinforcementlearningalgorithmsfortraining
intelligent agents to perform tasks in simulated environments [33], such as using the
Unityengine[34]. OneexampleistheuseofdeepRL,whichcombinestheuseofdeep
neuralnetworkswithRLalgorithmstoenablethelearningofcomplextasksfromhigh-
dimensional sensory input [35]. This approach has been applied to a range of tasks in

Information2023,14,290 3of22
simulatedenvironments,includingthetrainingofautonomousvehiclestonavigateroads
andtraffic[36].
AnothernotabledevelopmentinRLforsimulationenvironmentsistheuseofmulti-
agentRLalgorithms,whichenablethetrainingofmultipleagentstointeractandlearnfrom
oneanotherinasharedenvironment[37]. Thishasthepotentialtoenablethedevelopment
of more complex and realistic simulations, as well as to facilitate the training of agents
toperformcollaborativetasks. Inaddition,therehasbeenagrowinginterestintheuse
ofRLalgorithmsfortrainingautonomousracingagentstonavigatetracksinsimulation
environments[38].Theseapproachesofteninvolvetheuseofactor–criticalgorithms,which
learntopredictthevalueofdifferentactionsinagivenstateandusethisinformationto
guidetheselectionofactions[39]. Inanotherpaper,theauthors[40]extendtheGenerative
AdversarialImitationLearning(GAIL)approachofReinforcementlearning(whichuses
human-providedinputs)tosolveshortcomingswhentrainingseveralagents.Theypropose
parameterssharingGAILwhichprovessuperiortoGAILininteractingstablyinamulti-
agentenvironment. Inthepaper[41],theauthorpresentsanRL-basedapproachwhere
multipleagentscooperateandcoordinatetheiractionsinasimulatedtrafficenvironment.
TheauthorusesadeepRLtotrainsaidagentstoimproveandoptimizetheactionsmade
basedonactionsofotheragentsintheenvironment.Theauthorproposesarewardfunction
thattakesintoconsiderationnotonlytheperformanceofeachagent,butadditionallythe
totalperformanceofthewholesystem. Theauthoralsointroducesamechanismforthe
agentstocommunicateandshareinformationoftheactionsperformedincertainconditions
andwhatresultsthatleadsto. Temporalinformationandhistoricaldatacanalsobeusedto
trainagentstotraverseroadsandtracks. Inthepaper[42],theauthorsusedConvolutional
NeuralNetworks(CNN)toextractfeaturesfromtheroad,afterwhichanLSTM(longshort
termmemory)networkisusedtochooseanactionbasedonhistoricaldataofdifferent
actionstakenbasedondifferentfeaturesextracted. Theapproachwastestedontheopen
racingcarsimulatorandhasbeenabletomimichumandecisionswitharelativelyhigh
degreeofaccuracy.
Someapproachesusebothreal-worldandsimulatedenvironmentstotrainandim-
provemodelsinautonomousdrivingenvironments[43]. DeepandtraditionalRL-based
modelscanbetrainedonsimulatedagents,afterwhichthemodelsarefinetunedinreal-
worldenvironmentswherethemodelisfinetunedtoperformmoreinlinewithwhatis
expected [44]. The authors of [45] suggest a comprehensive learning approach for self-
drivingsystemswhichutilizesneuralnetworkstoapproximatesuitablemotorcommands
fromsensoryinput. Theauthorstackletheissueofreturningacartoitsdesignatedlane
whenitdeviatesoffcoursebygatheringrecoverydatabasedonthedistancefromapre-
ferredtrackwhileconductingaroadtestusingasimulator. Theproposedmethodconsists
ofthreephases: firstly,dataaregatheredbymeansofapath-followingmoduleduringa
hundredlapsofdriving;secondly,aneuraldrivingmoduleistrainedusingthesedatato
generatedrivingbehavior,suchasadjustingtheaccelerator,brakeandsteeringbasedona
particularthreshold;finally,theneuraldrivingmoduleisre-trainedusingdatacollected
fromthepath-followingmoduleduringanotherhundredlapsofdriving. Theefficacyof
theproposedapproachisassessedbycomparingtheaveragedistancefromthenearest
waypointlinkandtheaveragedistancetraveledperlapacrossdatasetswithnorecovery,
randomrecoveryandtheproposedmethodwithrecovery. Thefindingsdemonstratethat
themodelbasedontheproposedmethodperformedwellanddemonstratedagreaterfocus
ontheroadasopposedtounrelatedobjects,acrossbothtrainedanduntrainedcoursesand
variousweatherconditions.
Wecomparethediscussedstudiesbasedontheirapplicationdomain,reinforcement
learningalgorithmusedandperformancemetricsinTable1.

Information2023,14,290 4of22
Table1.Comparisonofstudiesbasedonapplicationdomain,reinforcementlearningalgorithmsand
performancemetrics.
| Study ApplicationDomain | RLAlgorithm | PerformanceMetrics |
| ----------------------- | ----------- | ------------------ |
torque,steering,
virtualvehicle acceleration,rapidity,
| [33] | PPOandBC |     |
| ---- | -------- | --- |
simulation revolutionsperminute
(RPM)andgearnumber
deepQ-learningwith
| [35] gameplaying |     | winrate |
| ---------------- | --- | ------- |
experiencereplay
| [36] autonomousdriving | -                   | autonomy                   |
| ---------------------- | ------------------- | -------------------------- |
| [37] robotics          | MADDPG              | communicationsuccess       |
|                        | softactor–criticand | angle,trackposition,speed, |
[38] autonomousdriving
|     | rainbowDQN | wheelspeeds,RPM |
| --- | ---------- | --------------- |
associativesearchelement
| [39] polebalancing | (ASE)andadaptivecritic | score |
| ------------------ | ---------------------- | ----- |
element(ACE)
ParameterSharing
| [40] autonomousdriving | GenerativeAdversarial | RMSE |
| ---------------------- | --------------------- | ---- |
ImitationLearning(GAIL)
successfulintersection
| [41] autonomousdriving | DQN |     |
| ---------------------- | --- | --- |
crossings
| [42] autonomousdriving | DQN | drivingdecisions |
| ---------------------- | --- | ---------------- |
| [43] robotics          | DQN | distancerun      |
A3C(Asynchronous
OpenAIGymbenchmark
| [44] robotics | AdvantageActor–Critic), |     |
| ------------- | ----------------------- | --- |
metrics
PPO
| [45] autonomousdriving | -   | distancetravelled |
| ---------------------- | --- | ----------------- |
ThestateoftheartinRLalgorithmsforsimulationenvironmentscontinuestoevolve,
withongoingresearchfocusedondevelopingmoreefficientandeffectiveapproachesfor
training intelligent agents to perform a wide range of tasks. RL algorithms have been
increasinglyusedtotrainagentstoperformtasksinsimulatedenvironments,suchasthe
Unityengine. DeepRL,whichcombinesdeepneuralnetworkswithRLalgorithms,has
beenusedtolearncomplextasksfromhigh-dimensionalsensoryinput,suchastraining
autonomousvehiclestonavigateroadsandtraffic. Multi-agentRLalgorithmshavealso
beendeveloped,allowingthetrainingofmultipleagentstointeractandlearnfromone
another in a shared environment. These have potential for the development of more
complexandrealisticsimulations,aswellasthetrainingofagentstoperformcollaborative
tasks. RLapproacheshavealsobeenusedtotrainautonomousracingagentstonavigate
tracksinsimulationenvironmentsusingactor–criticalgorithmsandtotrainmultipleagents
tocooperateandcoordinateactionsinasimulatedtrafficenvironment. CNNsandLSTMs
havebeenusedtoextractfeaturesfromtheroadandhistoricaldatatochooseactionsfor
autonomousdriving. Someapproachesusebothreal-worldandsimulatedenvironments
totrainandfinetunemodelsinautonomousdrivingenvironments.
3. MaterialsandMethods
3.1. ReinforcementLearningforAutonomousCartRacing
Reinforcementlearning(RL)isalearningframeworkwhereanagentlearnstomake
decisionsbyinteractingwithanenvironment. Theagent’sgoalistomaximizetheexpected
cumulativerewardovertime. Autonomouscartracingisataskwheremultipleagents,
representedbyautonomouscarts,navigatethroughatrackwhilecompetingagainsteach
othertoreachthefinishlineasfastaspossible. AmathematicaldefinitionofRLforthis
taskcanbedefinedasfollows:
Lettherebeasetofagents A = a 1 ,a 2 ,...,a n ,wherenisthetotalnumberofagents.
Eachagenta isanautonomouscartthatinteractswiththetrackenvironmentinasequence
i
of discrete time steps t = 1,2,...,T. At each time step t, each agent a takes an action
i

Information2023,14,290 5of22
a A,
i,t from a set of actions i which include acceleration, braking and steering and the
environmenttransitionstoanewstates andprovideseachagentwithascalarreward
t+1
|                                         |     |     | (a |s ),whichisaprobabilitydistributionover |     |     |     |
| --------------------------------------- | --- | --- | ------------------------------------------- | --- | --- | --- |
| r i,t . Theagent’sgoalistolearnapolicyπ |     |     | i i,t t                                     |     |     |     |
actionsgiventhecurrentstate,thatmaximizestheexpectedcumulativerewardovertime,
alsoknownasthereturn,definedas:
|     |     |      | (cid:104)      | (cid:105) |     |     |
| --- | --- | ---- | -------------- | --------- | --- | --- |
|     | J(π | ) =E | π ∑ t =0T−1γtr |           |     | (1) |
|     |     | i    | i              | i,t       |     |     |
∈ [0,1]
where γ is a discount factor that determines the importance of future rewards
andr istherewardassociatedwithreachingthefinishlineasfastaspossible,avoiding
i,t
collisionsandpenalties. Thiscanalsobedefinedbyconsideringthestatevaluefunction
V (s )whichrepresentstheexpectedtimetoreachthefinishlinestartingfromstates and
πi t t
followingpolicyπ: i
|     |        |     | (cid:104)    | (cid:12)           |     |     |
| --- | ------ | --- | ------------ | ------------------ | --- | --- |
|     | (s )   | =E  | ∑ =0T−t−1γkr | (cid:12) ]         |     |     |
|     | V πi t | π   | i k          | i,t+k (cid:12) s t |     | (2) |
andtheaction-valuefunctionQ (s ,a )whichrepresentstheexpectedtimetoreachthe
|     | πi  | t   | i,t |     |     |     |
| --- | --- | --- | --- | --- | --- | --- |
finishlinestartingfromstates ,takingactiona
|      | t             |     | i,t andfollowingpolicyπ: |                             | i   |     |
| ---- | ------------- | --- | ------------------------ | --------------------------- | --- | --- |
|      |               |     | (cid:104)                | (cid:12)                    |     |     |
|      |               | =E  | ∑ =0T−t−1γkr             | (cid:12)                    |     |     |
| Q πi | (s t ,a i,t ) | π   | i k                      | i,t+k (cid:12) s t ,a i,t ] |     | (3) |
TheobjectiveofRLformultipleagentsinautonomouscartracingisforeachagenta i
tolearnapolicyπ thatmaximizesitsownexpectedtimetoreachthefinishlineasfastas
i
possiblewhileavoidingcollisionsandpenalties.
3.2. TestEnvironment
Thesimulation/testenvironmentwaschosentobetheUnitygameengine. Theexper-
imentsconductedforthisprojectusedapubliclyavailableenvironment(repositoryname
https://github.com/jaredbest/unity-ai-racing-karts-ml-agentsaccessedon8April2023).
Thisenvironmentincludesaracingtrackaswellasthekarts/agents. Thereare24agentsin
total. Multipleagentsareusedtospeeduptraining. Agentsareindependentofeachother,
meaningtheirtrainingoccursindependently. Anillustrationoftheenvironmentcanbe
seenbelowinFigure1.
Figure1.IllustrationoftheUnityenvironmentusedtotest.
Theenvironmentalsoincludeskartmodelsthatareabletotraversethetrack. There
are24suchkartsandtheyareusedastheagentsinourexperiments. Therearethreeways
bywhichtheagentscanbecontrolled,oneisthroughtheusageofML-agents,bywhich
theML-agent’sso-called“brain”isusedtocontroltheagents. Thisisthedefaultso-called

Information2023,14,290 6of22
“behaviortype”. Thesecondoptionisthe“inferenceonly”behaviortype,withwhichthe
agentsuseanalreadytrainednetworktocontroltheagents. Thefinalbehaviortypeis
called“heuristiconly”;withit,ausercontrolstheagentusingpre-specifiedkeys. Allthree
typesareusedduringourexperimentation. The24mentionedkarts/agentscanbeseenin
Figure2.
Figure2.Agentsfortraining.
Thesecondpartoftheexperimentsincludedaddingobstaclesonthetracktoseeif
itispossibletotrainagentsthatcanbothtraversethetrackaswellasavoidallobstacles.
Theobstaclesusedaresimpleroundroadblocks. Theyareplacedrandomlyalongthetrack.
Their positions will change for some of the experiments to showcase the robustness of
trainedmodels. SomeoftheobstaclescanbeseeninFigure3.
Figure3.Asnapshotofsomeoftherandomlyplacedobstacles.
3.3. Algorithms
In this paper two main algorithms are used, with some modifications of each to
trainouragents. Thetwoalgorithmsarethewell-knownProximalPolicyOptimization
(PPO) [46] and POCA [47] algorithms. Those algorithms are used as they are available
in the ML-agents framework used. Both of the algorithms support multi-agent train-
ing. Training multiple independent agent speeds up training and makes good use of
distributedcomputing.

Information2023,14,290 7of22
3.3.1. MA-PPOAlgorithm
TheMA-PPO(Multi-AgentProximalPolicyOptimization)algorithmisavariantof
thePPO(ProximalPolicyOptimization)algorithmthatisspecificallydesignedfortraining
multipleagentsinasharedenvironment. PPOisareinforcementlearningalgorithmthat
usesacombinationofvalueandpolicygradientstooptimizetheperformanceofanagent
inanenvironment. Itisknownforbeingstableandrelativelyeasytoimplementcompared
tootherreinforcementlearningalgorithms. TheMA-PPOalgorithmextendsthestandard
PPOalgorithmtoworkwithmultipleagents,allowingthemtolearnandinteractwitheach
otherwithinasharedenvironment.Thiscanbeusefulfortrainingagentstocoordinatetheir
actions,suchasinmulti-agentgamesorsimulations,includingforthetaskofautonomous
cartracing. TheMA-PPOalgorithmcanbedefinedasfollows:
Lettherebeasetofagents A = a ,a ,...,a ,wherenisthetotalnumberofagents.
1 2 n
Eachagenta hasapolicyπ (a |s ;θ )thatisparameterizedviaθ,whereπ isaprobability
i i i,t t i i i
distributionoveractionsgiventhecurrentstate. TheobjectiveoftheMA-PPOalgorithmis
tofindthepolicyparameterθ thatmaximizestheexpectedcumulativerewardovertime,
i
alsoknownasthereturn,definedas:
(cid:104) (cid:105)
J(π ) =E π ∑ t =0T−1γtr (4)
i i i,t
whereγ ∈ [0,1]isadiscountfactorthatdeterminestheimportanceoffuturerewards.
TheMA-PPOalgorithmupdatesthepolicyparametersθ bymaximizingasurrogate
i
objectivefunctionLMA−PPO(θ )definedas:
i
(cid:34) (cid:32) (cid:33)(cid:35)
π (a |s ;θ )
LMA−PPO(θ i ) =E τ∼πi min r t (θ i ) π ( i a | t s ; t θol i d) ,clip(r t (θ i ),1−(cid:101),1+(cid:101)) (5)
i t t i
whereτisthetrajectoryoftheagent,r (θ )isthelikelihoodratiobetweenthenewandold
t i
policy,π (a |s ;θ )andπ (a |s ;θold),respectively,and(cid:101)isahyperparameterthatcontrols
i t t i i t t i
thestepsize.
TheMA-PPOalgorithmrepeatedlyupdatesthepolicyparametersθ byperforming
i
gradientascentonthesurrogateobjectivefunctionLMA−PPO(θ )usingmini-batchoftrajec-
i
toriessampledfromthecurrentpolicy. Insummary,MA-PPOisavariantofPPOthatcan
beusedtotrainmultipleagentssimultaneouslyforthetaskofautonomouscartracingby
updatingthepolicyparametersθ withthegoalofmaximizingtheexpectedcumulative
i
reward over time. The MA-PPO algorithm has been shown to be effective in a variety
ofenvironments,includingmulti-agentgamesandcooperativetasks[48]. Asimplified
diagramofhowthePPOalgorithmworkscanbeseeninFigure4.
Figure4.MAPPOworkflowdiagram.

Information2023,14,290 8of22
ThepseudocodeofthePPOcanbeseeninAlgorithm1:
Algorithm1MA-PPOalgorithm
1: InitializeapolicynetworkπandavaluenetworkV
Initializeasetofparametersθforthepolicyandvaluenetworks
2:
| 3: Initializeasetofoldparametersθ | old forthepolicyandvaluenetworks |     |     |
| --------------------------------- | -------------------------------- | --- | --- |
| 4: Initializeasetoftrajectoriesτ  | = {}                             |     |     |
foreachiterationdo
5:
6: Resettheenvironmentandgetinitialstates
0
foreachtimestepdo
7:
| 8: Sampleanactiona | t fromthecurrentpolicyπ | θ            |     |
| ------------------ | ----------------------- | ------------ | --- |
| 9: Executeactiona  | andobserverewardr       | andnewstates |     |
|                    | t                       | t            | t+1 |
Storethetransitiontuple(s ,a ,r ,s )inτ
| 10: | t t t | t+1 |     |
| --- | ----- | --- | --- |
11: Updatethevaluenetwork: V (s t ) ←V (s t )+α(r t +γV (s t+1 )−V (s t ))
|     | θ   | θ   | θ θ |
| --- | --- | --- | --- |
endfor
12:
←compute_advantages(τ,γ,λ))
13: CalculatetheadvantagesusingtheGAE(τ
14: Normalizetheadvantages
| Updatetheoldpolicyparameters: | θ   | ← θ |     |
| ----------------------------- | --- | --- | --- |
| 15:                           | old |     |     |
16: OptimizethepolicyusingthePPOobjective: θ ←optimize_policy(τ,θ,θ )
old
endfor
17:
3.3.2. POCAAlgorithm
POCA(ParallelOnlineContinuousArcing)[49]isaboostingalgorithmthatdiffers
fromtraditionalarcingalgorithmssuchasAdaboost. Whiletraditionalarcingalgorithms
constructanensemblebyaddingandtrainingweaklearnerssequentiallyonaround-by-
roundbasis,POCAperformstrainingoveranentireensemblecontinuouslyandinparallel.
ThisallowsPOCAtoadaptrapidlytonon-stationaryenvironments,asmembersofthe
ensemble are not frozen after an initial learning period. Additionally, POCA does not
requiretheexplicitstorageofexemplarstatistics,makingitcapableofonlinelearning. Asa
result,POCAisaboostingalgorithmthattrainsanensembleofweaklearnersinparallel
and continuously, enabling fast adaptation to non-stationary environments and online
learningcapabilities. InFigure5,asimplifiedviewofthewayPOCAalgorithmworkscan
beseen. ThepseudocodecanbeseeninAlgorithm2.
Figure5.POCAdiagram.

Information2023,14,290 9of22
Algorithm2POCA
| 1: Initializeasetofpoliciesπ       |     |     |     | 1 ,π 2 ,...,π | N foreachagent |              |     |     |     |
| ---------------------------------- | --- | --- | --- | ------------- | -------------- | ------------ | --- | --- | --- |
| 2: InitializeasetofvaluefunctionsV |     |     |     |               | ,V ,...,V      | foreachagent |     |     |     |
|                                    |     |     |     |               | 1 2            | N            |     |     |     |
3: Initializeasetofparametersθ 1 ,θ 2 ,...,θ N forthepolicyandvaluefunctionsofeach
agent
Initializeasetofoldparametersθ ,θ ,...,θ forthepolicyandvaluefunctions
| 4:  |     |     |     |     | old,1 old,2 | old,N |     |     |     |
| --- | --- | --- | --- | --- | ----------- | ----- | --- | --- | --- |
ofeachagent
| 5: Initializeasetoftrajectoriesτ |     |     |     | =   | {}  |     |     |     |     |
| -------------------------------- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
foreachiterationdo
6:
7: Resettheenvironmentandgetinitialstates
0
foreachtimestepdo
8:
9: Sampleactionsa 1,t ,a 2,t ,...,a N,t fromthecurrentpoliciesπ 1,θ1 ,π 2,θ2 ,...,π N,θN
10: Executeactionsa ,a ,...,a andobserverewardsr ,r ,...,r andnew
|     |     |     | 1,t | 2,t | N,t |     | 1,t | 2,t | N,t |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
states
t+1
11: Storethetransitiontuples(s t ,a ,a ,...,a ,r ,r ,...,r ,s t+1 )inτ
|     |                          |     |     |     | 1,t 2,t | N,t 1,t     | 2,t | N,t  |            |
| --- | ------------------------ | --- | --- | --- | ------- | ----------- | --- | ---- | ---------- |
|     | Updatethevaluefunctions: |     |     |     | V (s )  | ←V (s )+α(r | +γV | (s   | )−V (s ))  |
| 12: |                          |     |     |     | i,θi t  | i,θi t      | i,t | i,θi | t+1 i,θi t |
forallagentsi
13: endfor
CalculatetheadvantagesusingtheGAE(τ ←compute_advantages(τ,γ,λ))
14:
15: Normalizetheadvantages
| 16: | Updatetheoldpolicyparameters: |     |     |     | θ     | ← θ forallagentsi |     |     |     |
| --- | ----------------------------- | --- | --- | --- | ----- | ----------------- | --- | --- | --- |
|     |                               |     |     |     | old,i | i                 |     |     |     |
←optimize_policy(τ,θ
17: OptimizethepoliciesusingtheMPOCAobjective: θ i i ,θ old,i )
forallagentsi
endfor
18:
3.4. RewardStructureoftheImplementation
Therewardstructureoftheenvironmentwasnoteditedexceptfortheadditionof
punishment/negativerewardinthecaseoftheaddedobstacles. Thelistbelowexplains,
withoutgoingintodetail,howtheepisodesareportrayedandwhenrewardsorpunish-
mentsareadded.
1. AgentsbeginatthestartingpositionwheretheML-agents’‘brain’startslisteningto
inputandprovidesactionsforagentstoperform.
2. Wheneveranagentpassesthroughacheckpoint,arewardisaddedtotheagent’s
totalthatequalsthe0.5/n,nherebeingthetotalnumberofcheckpoints.
3. If the time to reach the next checkpoint exceeds 30 s, the episode ends, the agent
receivesapunishmentof−1andtheagentrespawnsatthestartofthetrack.
4. Whenevertheagentreachesthefinalcheckpoint,arewardof0.5isgiven,theepisode
endsandtheagentrespawnsatthestartingposition.
Toincentivizespeed,agentsaregivenasmall−0.001reward(punishment).
5.
6. Inthecaseoftheaddedobstaclesversionoftheenvironment,anegativerewardof
−0.1isgiveneverytimeacollisionoccursbetweentheagentandanyoftheobstacles.
ThiscanalsobesummarizedinthepseudocodeavailableinAlgorithm3.
3.5. AgentsSequenceDiagram
The sequence diagram in Figure 6 shows how the ML-agent clients use the ML-
Agents Server to understand the environment. If the agent successfully navigates past
checkpointsandobstacleswith Action(A ),itreceivesaReward(R )fromtheML-Agents
|         |                                                                          |     |     |     | t   |     | t   |     |     |
| ------- | ------------------------------------------------------------------------ | --- | --- | --- | --- | --- | --- | --- | --- |
| server. | However,iftheagentisnotefficient,itreceivesaPunishment(P)andreturnstothe |     |     |     |     |     |     | t   |     |
serverwithaState(S )toreceivethenext Action(A )andReward(R ). Thisprocess
|     |     | n   |     |     |     | t+1 |     | t+1 |     |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
repeats multiple times, with the agent receiving different Action(A t+n ), Reward(R t+n ),
| Punishment(R |     | )andState(S |     | ).  |     |     |     |     |     |
| ------------ | --- | ----------- | --- | --- | --- | --- | --- | --- | --- |
|              |     | t+n         |     | t+n |     |     |     |     |     |

Information2023,14,290 10of22
Algorithm3Rewardstructure
1: Input: kartagents,track,obstacles(optional)
2: Output: trainedkartagents
3: Initialize: agentsatstartingposition,totalreward=0,totalnumberofcheckpoints=n
4: repeat
5: Beginepisode:
6: whileagenthasnotreachedfinalcheckpointdo
7: agentperformsactionsbasedoninputandbrain’soutput
8: ifagentpassesthroughcheckpointthen
9: totalreward+=0.5/n
10: endif
11: iftimetoreachnextcheckpoint>30sthen
12: endepisode
13: agentreceivespunishmentof−1
14: agentsrespawnatstartoftrack
15: endif
16: ifagentcollideswithobstacles(addedversiononly)then
17: totalreward+=−0.1
18: endif
19: agentreceivespunishmentof-0.001foreverytimestep
20: endwhile
21: Endepisode:
22: agentreceivesrewardof0.5
23: agentsrespawnatstartoftrack
24: untilagentsaresufficientlytrained
Figure6.ML-agentSequenceDiagram.

Information2023,14,290 11of22
4. ExperimentalEvaluationandResults
4.1. Settings
Theevaluationofthedifferentexperimentscarriedoutiscarriedoutbycomparing
themeanrewardforallkartsinthefinalstep. Theresultsoftherewardsoftheexperiments
carriedoutwithandwithoutobstaclesshouldnotbedirectlycompared,ashavingobstacles
slowsdownandthusaddsasignificantnegativerewardtoagents. Thus,wefirstcompare
differentalgorithmsusingthedefaultenvironmentwhereobstacleswerenotaddedand
selectthealgorithm/modelwiththehighestmeanreward. Forexperimentsthatinclude
obstacles,thebestalgorithmfromthepreviousstepsisusedtotraintheagents.Experiments
carriedoutintheenvironmentwithaddedobstaclesonthetrackareevaluatedseparately.
The experiments shall be divided into two main sections and further subsections.
Thesearelistedbelow.
• Environmentwithoutobstacles.
– Defaultmodels.
| *   | DefaultPPOalgorithmconfigurations. |     |     |     |     |
| --- | ---------------------------------- | --- | --- | --- | --- |
DefaultPOCAalgorithmconfigurations.
*
| *   | ML-agentsdefault,whichalsousesthePPOalgorithm. |     |     |     |     |
| --- | ---------------------------------------------- | --- | --- | --- | --- |
AddingRNNtothebestmodelfromthedefaultmodels.
*
• Environmentwithobstacles.
– DefaultPPOalgorithm.
– Addingbehavioralcloningasapre-trainingconditionwiththedefaultPPOalgorithm
Beforegoingintotheresults, wemustfirstlookatthetoolsusedtoperformthose
| experiments. | InTable2,boththehardwareandsoftwareusedcanbeseen. |     |     |     |     |
| ------------ | ------------------------------------------------- | --- | --- | --- | --- |
Table2.Hardwareandsoftwareused.
|          |     |           | Video  | Memory |     |
| -------- | --- | --------- | ------ | ------ | --- |
| Hardware | GPU | Pipelines |        |        |     |
|          |     |           | Memory | Type   |     |
Nvidia1650
|     |     | 1024 | 4GB | GDDR6 |     |
| --- | --- | ---- | --- | ----- | --- |
ti
ML-Agents
|          | UnityEditor |         | Pytorch | CUDA    | Python  |
| -------- | ----------- | ------- | ------- | ------- | ------- |
| Software |             | Package |         |         |         |
|          | Version     |         | Version | Version | Version |
Version
|     | 2020.3.39f1 | 0.29.0 | 1.8.0+cu111 | 11.4 | 3.8.0 |
| --- | ----------- | ------ | ----------- | ---- | ----- |
4.2. Results
4.2.1. EnvironmentwithoutObstacles
First we train and compare different models in the default environment without
obstacles. Thiswillbedividedintotwo,thedefaultmodelsandthebestmodelchosenwith
addedRNAas(1)defaultmodelsand(2)defaultmodeloftheML-agents.
First, the default ML-agents model was experimented with. The results were not
satisfactoryastheagentswereunabletonavigatethetrackeasilyanddidnotproduce
decentrewardsorlossofvalue. Themodelyieldedameancumulativerewardof−1.582
andavaluelossof0.006. TheplotsofthemodelcanbeseeninFigure7. Thismodeluses
theconfigurationsseeninTable3.

Information2023,14,290 12of22
(a) (b)
Figure7.DefaultML-agentsmodelresultplots:(a)rewardvalue,(b)lossvalue.
Table3.ML-agent’sdefaultmodelconfigurations.
Parameter Value
batch_size 1024
buffer_size 10,240
learning_rate 0.0003
beta 0.005
epsilon 0.2
lambda 0.95
num_epoch 30
learning_rate_schedule linear
4.2.2. DefaultPPOModel(AlsoEnvironment’sDefault)
ThedefaultPPOmodelwasexperimentedwithandyieldedthebestresultswitha
meancumulativerewardandvaluelossof0.761and0.0013,respectively. Theplotsofthe
modelcanbeseeninFigure8. ThismodelusestheconfigurationsseeninTable4.
(a) (b)
Figure8.DefaultPPOresultplots:(a)rewardvalue,(b)lossvalue.

Information2023,14,290 13of22
Table4.ConfigurationsofthedefaultPPOmodel
Parameter Value
batch_size 120
buffer_size 12,000
learning_rate 0.0003
beta 0.001
epsilon 0.2
lambda 0.95
num_epoch 30
learning_rate_schedule linear
4.2.3. DefaultPOCAModel
ThedefaultPOCAmodelachieveddecentresultswithacumulativerewardandvalue
lossof−0.372and0.002,respectively. TheplotsofthemodelcanbeseeninFigure9. This
modelusestheconfigurationsseeninTable4.
(a) (b)
Figure9.DefaultPOCAmodelresultplots:(a)rewardvalue,(b)lossvalue.
AddingmemoryorRNNblockstothemodelwasattemptedwiththeresultsshown
below. The cumulative reward degraded with the number of steps and did not reach
satisfactoryresultsafterthesetnumberofstepswithafinalrewardof−2.411andalossof
0.082. TheplotsofthemodelcanbeseeninFigure10. Thismodelusestheconfigurations
seeninTable5.
(a) (b)
Figure10.AddingRNNtothedefaultPPOmodel:(a)rewardvalue,(b)lossvalue.

Information2023,14,290 14of22
Table5.PPOmodelwithaddedmemory.
Parameter Value
batch_size 1024
buffer_size 10,240
learning_rate 0.0003
beta 0.005
epsilon 0.2
lambda 0.95
num_epoch 30
learning_rate_schedule linear
memory_size 128
sequence_length 64
Thenextsetofexperimentswillincludeanenvironmentwhereobstacleswereplaced
inrandompositions,whereagentswouldberequiredtotraversethetrackandavoidany
obstaclesintheirway.
4.2.4. DefaultPPOModel
The default PPO was retrained using an environment for which obstacles were
added.The results were not satisfactory, with a final mean reward of −2.574 and a loss
valueof0.018. TheplotsofthemodelcanbeseeninFigure11. Theconfigurationsused
canbefoundinTable4.
(a) (b)
Figure11.DefaultPPO(obstacleenvironment)resultplots:(a)rewardvalue,(b)lossvalue.
ThedefaultPPOwasretrainedusinganenvironmentinwhichobstacleswereadded.
Behavioralcloningwasusedhereasapre-trainingconditionwithastrengthhyperparame-
tersetto0.1. Theresultswerenotsatisfactorywithafinalmeanrewardof−2.547anda
lossvalueof0.0042. TheplotsofthemodelcanbeseeninFigure12. Theconfigurations
usedcanbefoundinTable4.

Information2023,14,290 15of22
(a) (b)
Figure12.DefaultPPO(obstacleenvironment)resultplotswithbehavioralcloning(strengthof0.1):
(a)rewardvalue,(b)lossvalue.
Afteraddingbehavioralcloningasapre-trainingconditionwithastrengthhyper-
parametersetto1.0,agentswereabletolearnthedesiredbehavior. Theresultswerenot
satisfactory,withafinalmeanrewardof0.0681and0.0011. Plotsofthemodelcanbeseen
inFigure13. TheconfigurationsusedcanbefoundinTable4.
(a) (b)
Figure13.DefaultPPO(obstacleenvironment)resultplotswithbehavioralcloning(strengthof1.0):
(a)rewardvalue,(b)lossvalue.
4.2.5. ComparingtheFinalModelonDifferentObstaclePositions
Asarobustnesstest,thefinalmodeltrainedontheobstaclesthatincludestheobsta-
cles (17 obstacles in total) was tested on three different random configurations that are
listedbelow:
1. Firstconfiguration: thefigureshowstheconfigurationthatthemodelwastrained
withFigure14a.
2. Second configuration: a configuration in which obstacles were placed in different
randompositions,ascanbeseeninFigure14b.
3. Thirdconfiguration: anotherconfigurationinwhichobstacleswereplacedagainin
differentrandompositionsandthiscanbeseeninFigure14c.

Information2023,14,290 16of22
(a) (b) (c)
Figure14.Configurations:(a)first,(b)second,(c)third.
Tocomparethemodelwiththepositionsoftheaforementionedobstacles,werunthe
modelininferencemodeforoneminuteperconfiguration. Themetricusedhereissimply
thenumberoftimestheagentcollideswithanobstacle. Itisimportanttonoteherethatthe
teststhatfollowwerecarriedoutonasingleagent(allotheragentsweredisabled). Thisis
doneforbetterinterpretabilityandfordemonstrationpurposes. Theresultsarepresented
inFigure15.
Figure15.Comparisonofdifferentobstacleconfigurations.
4.2.6. ComparingModelSizes
Tocomparethesizeofthemodels,weusetheactualmemorysizeofeachmodelin
kilobytes. Thesedorepresentthesizesofthemodelsdirectlyasthefileformatusedtostore
themodelsisnotcompressedandisstoredasis. Thecomparisonofthedifferentmodel
sizescanbeseeninFigure16. EverymodelthatusesthedefaultPPOhyperparametershas
thesamesize(hencewhyweonlyhavefourbars).

Information2023,14,290 17of22
Figure16.Comparisonofmodelsizes.
5. DiscussionandFutureResearch
5.1. EvaluationofFindings
Theresultsofourexperimentsconfirmtheusefulnessofbehavioralcloningforim-
proving the performance of intelligent agents for racing tasks. Behavioral cloning is a
techniquewhereanagentistrainedtomimicthebehaviorofanexpert. Inthecontextof
thispaper,itinvolvestraininganagentusingadatasetofexpertdemonstrationswhere
ahuman-controlledkartnavigatestheracingtrackandavoidsobstacles. Addingbehav-
ioralcloningasapre-trainingconditionwiththedefaultPPOalgorithmcanimprovethe
performanceofintelligentagentsforsolvingtheracingtaskinseveralways.
• First, pre-training with behavioral cloning can help to initialize the agent’s policy
networkwithasetofgoodinitialweights. Thiscanhelptoimprovetheconvergence
speed of the RL algorithm during training, allowing the agent to learn faster and
achievebetterperformance.
• Secondly,behavioralcloningcanhelptoimprovetheagent’sabilitytogeneralizeto
newsituationssuchasdifferentobstacleconfigurations. Bytrainingtheagentona
dataset of expert demonstrations that includes a variety of different scenarios and
obstacleconfigurations(seeFigure15),theagentcanlearntorecognizeandrespond
appropriatelytodifferentsituationsitmayencounterduringtheracingtask. Thiscan
helptoimprovetheagent’soverallperformanceandreducethelikelihoodofitgetting
stuckinlocaloptimaduringtraining.
• Finally,addingbehavioralcloningasapre-trainingconditionwiththedefaultPPO
algorithm can improve the stability and robustness of the agent’s policy network.
Bytrainingtheagenttomimicthebehaviorofanexpert,theagentcanlearntoavoid
certainmistakesorsuboptimalbehaviorsthatmayariseduringtheRLtrainingprocess.
Thiscanhelptoimprovetheoverallqualityoftheagent’spolicynetworkandmakeit
moreresistanttonoiseandothersourcesofvariabilityintheenvironment.
Insummary,addingbehavioralcloningasapre-trainingconditionwiththedefault
PPOalgorithmcanimprovetheperformanceofintelligentagentsforsolvingthecartracing
taskbyimprovingtheinitializationofthepolicynetwork,improvingtheagent’sabilityto
generalizetonewobstacleconfigurationsandimprovingthestabilityandrobustnessof
themodel.

Information2023,14,290 18of22
5.2. NetworkSimplificationUsingPruningTechniques
Networkpruningtechniquescanbeusedtosimplifydeepnetworksusedfortraining
ML-agentsusingRL,whichcanhelptoreducethenumberofparametersandmemory
usage [50–52]. Deepneuralnetworkstypicallyconsistofmillionsoftrainableparameters,
whichcanmakethemcomputationallyexpensiveanddifficulttotrain. Networkpruning
techniquesinvolveremovingunnecessaryconnectionsorneuronsfromanetwork,which
canreducethenumberofparametersandimprovetheefficiencyofthenetwork. Thereare
severaldifferentnetworkpruningtechniquesthatcanbeused,includingweightpruning,
neuron pruning and filter pruning. Weight pruning involves removing small-weight
connectionsfromthenetwork,whileneuronpruninginvolvesremovingentireneurons
that are not contributing significantly to the network’s output. Filter pruning involves
removing entire filters from convolutional layers that are not contributing significantly
tothenetwork’soutput. Byusingnetworkpruningtechniques,itispossibletosimplify
deepnetworksusedfortrainingML-agentsusingRL,whichcanreducethenumberof
parametersandmemoryusage. Thiscanmakethenetworksmoreefficientandeasierto
train, which can ultimately lead to better performance. Additionally, network pruning
canhelptoreducetheriskofoverfitting,asitcanpreventthenetworkfrommemorizing
noiseinthetrainingdata. Overall,networkpruningtechniquescanbeausefultoolfor
simplifyingdeepnetworksusedfortrainingML-agentsusingRL.Byreducingthenumber
ofparametersandmemoryusage,networkpruningcanmakethenetworksmoreefficient
andeasiertotrain,whichcanultimatelyleadtobetterperformance.
5.3. PossibleApplications
TheuseoftheUnityML-Agentstoolkittotrainintelligentagentstonavigatearacing
trackinasimulatedenvironmentusingRLalgorithmshasseveralpotentialreal-world
applications. One possible application is in the development of autonomous vehicles,
whereRLalgorithmscanbeusedtotrainagentstonavigatecomplexenvironmentsand
avoidobstacles. Theuseofasimulatedenvironmentallowsforsafeandefficienttesting
ofautonomousvehiclesystemsbeforetheyaredeployedonrealroads. Anotherpotential
applicationisinthedevelopmentofrobotics,whereRLalgorithmscanbeusedtotrain
robotstoperformcomplextasksinavarietyofenvironments. Forexample,robotscouldbe
trainedtonavigatethroughclutteredenvironments,suchaswarehousesorfactories,toper-
formtaskssuchaspickingandpackingitems. TheuseofRLalgorithmstotrainintelligent
agentsinsimulatedenvironmentscanalsohaveapplicationsinthefieldofgaming. Game
developers can use these algorithms to create more intelligent and realistic non-player
characters(NPCs)[53,54]thatcaninteractwithplayersinmorecomplexways. Overall,
theuseoftheUnityML-AgentstoolkittotrainintelligentagentsusingRLalgorithmshas
thepotentialtorevolutionizeseveralindustries,includingautonomousvehicles,robotics
andgaming.
5.4. FutureResearch
Therearemanyareaswherethisresearchcouldbeexpandedon.Ourresearchhasbeen
veryspecificto,asisclearto,oneenvironmentandtooneframework(ML-agents)which
has a limited number of algorithms to choose from. To summarize areas of expansion
for future research, we list them below and go into some detail concerning what each
wouldmean.
• Expansionofthealgorithmsusedandhyperparametersexperimentedwith. Asmen-
tionedabove,ML-agentsonlyprovideasmallsubsetofalgorithmstochoosefrom.
Itdoessimplifyexperimentationandmakesitmoreconvenientforanyresearcher
while being very user-friendly with great documentation and a large community.
However,itdoesnotexplorethelargenumberofalgorithmsavailable. Itisagreat
tool/framework,butdoeshavelimitations.
• Environment augmentation. There is little research in this particular area. Laskin
et al. [55] proposed the enhancement of input data that agents receive, but do not

Information2023,14,290 19of22
exactly go into the enhancement of the environment. A proposed methodology
would include either different random changes to the environment which would
prevent the agents from overfitting into the environment they are trained in (this
couldevenbetotallydifferentenvironmentstrainedon). Agentsfindoptimalpaths
to complete the tasks, making it harder to generalize to different environments or
setups. Augmentationof this kindcan help generalize the modelso that different
tracksarecompletedunderdifferentconditions. Examplesofsuchanaugmentation
aregivenbelow:
– Differentescapepositionsforagentsduringtraining. Insteadofrespawningin
thesamearea,agentscanrespawnandrestartepisodesinrandompositionsin
randomorientations. Thiscouldpreventoverfitting.
– Changingthepositionsoftheobstaclesduringtraining. Ascanbeseeninthe
results,differentpositionsofobstacles(oralargernumberofsuchobstacles)than
what it has been trained on make it more difficult for the agents to avoid the
saidobstacles. Thiswouldalsodecreaseoverfittingandhelpgeneralizetoany
positionofanobstacle.
– Using completely different environments during training. This would be the
mostchallengingtask,asthiswouldrequiremuchmorerobustandmuchlarger
models. This,however,wouldalmostcertainlypreventanyoverfittingtoany
oneenvironment.
6. Conclusions
Inthispaper,weexploretheuseoftheUnityML-Agentstoolkittotrainkartagents
tonavigatearacingtrackinasimulatedenvironmentusingreinforcementlearning(RL)
algorithms. WehavecomparedtheperformanceofseveraldifferentRLalgorithmsand
configurationsonthetaskoftrainingkartagentstosuccessfullytraversearacingtrack
andhaveidentifiedthemosteffectiveapproachfortrainingkartagentstonavigatearacing
trackandavoidobstaclesinthattrack.
Ingeneral,ourfindingshaveimportantimplicationsforthedesignandimplemen-
tation of intelligent agents in racing simulations. Our results provide insight into the
capabilitiesandlimitationsofdifferentRLalgorithmsandcaninformthedevelopmentof
moreeffectiveandefficientapproachestotrainingintelligentagentsinsimulatedenviron-
ments. Wedrawonavarietyofsources,includinginouranalysisandconclusions.
1. Differentmodelsweretrainedandtheresultswererecorded. Thebestmodelturned
outtobethedefaultenvironment,whichusesthePPOalgorithm.Themodelproduces
alossvalueof0.0013andacumulativerewardof0.761forthefinalstep.
2. Adding obstacles and retraining using the best algorithm found did not produce
satisfactory results. AI agents were unable to find a policy that results in decent
rewards. The reward and loss at the final step of this model were found to be
−1.720and0.0153,respectively. Toassistthemodelinlearningtherequiredbehavior,
behavioralcloningwasusedasapre-trainingcondition. Arecordingofthedesired
behaviorwasmadeusingphysicalinputfromtheauthors. Usingbehavioralcloning,
themodelwasabletoachievesatisfactoryresultswheretheagentswereabletoavoid
obstacles and complete the track. The reward and loss for these were 0.0681 and
0.0011,respectively.
Author Contributions: Conceptualization, R.M. (Rytis Maskeliu¯nas); Data curation, R.M. (Rytis
Maskeliu¯nas)andR.D.;Formalanalysis,Y.S.,R.M.(RezaMahmoudi),R.M.(RytisMaskeliu¯nas)and
R.D.; Fundingacquisition,R.M.(RytisMaskeliu¯nas); Investigation,Y.S.,R.M.(RezaMahmoudi),
R.M.(RytisMaskeliu¯nas)andR.D.;Methodology,R.M.(RytisMaskeliu¯nas);Projectadministration,
R.M. (Rytis Maskeliu¯nas); Resources, Y.S. and R.M. (Reza Mahmoudi); Software, Y.S. and R.M.
(RezaMahmoudi);Supervision,R.M.(RytisMaskeliu¯nas);Validation,Y.S.,R.M.(RytisMaskeliu¯nas)
andR.D.;Visualization,Y.S.andR.M.(RezaMahmoudi);Writing—originaldraft,Y.S.,R.M.(Reza

Information2023,14,290 20of22
Mahmoudi)andR.M.(RytisMaskeliu¯nas);Writing—reviewandediting,R.M.(RytisMaskeliu¯nas)
andR.D.Allauthorshavereadandagreedtothepublishedversionofthemanuscript.
Funding:Thisresearchreceivednoexternalfunding.
DataAvailabilityStatement:Thedataisavailablefromthecorrespondingauthoruponreasonable
request.
Acknowledgments: Theauthorsacknowledgetheuseofartificialintelligencetoolsforgrammar
checkingandlanguageimprovement.
ConflictsofInterest:Theauthorsdeclarenoconflictofinterest.
References
1. Arulkumaran,K.;Deisenroth,M.P.;Brundage,M.;Bharath,A.A. Deepreinforcementlearning: Abriefsurvey. IEEESignal
Process.Mag.2017,34,26–38.[CrossRef]
2. Elguea-Aguinaco,Í.;Serrano-Muñoz,A.;Chrysostomou,D.;Inziarte-Hidalgo,I.;Bøgh,S.;Arana-Arexolaleiba,N. Areviewon
reinforcementlearningforcontact-richroboticmanipulationtasks. Robot.Comput.-Integr.Manuf.2023,81,102517.[CrossRef]
3. Malleret,T.;Schwab,K. GreatNarrative(theGreatResetBook2);WorldEconomicForum:Colonie,Switzerland,2021.
4. Crespo,J.;Wichert,A. Reinforcementlearningappliedtogames. SNAppl.Sci.2020,2,824.[CrossRef]
5. Liu,H.;Kiumarsi,B.;Kartal,Y.;TahaKoru,A.;Modares,H.;Lewis,F.L. ReinforcementLearningApplicationsinUnmanned
VehicleControl:AComprehensiveOverview. UnmannedSyst.2022,11,17–26.[CrossRef]
6. Jagannath,D.J.;Dolly,R.J.;Let,G.S.;Peter,J.D. AnIoTenabledsmarthealthcaresystemusingdeepreinforcementlearning.
Concurr.Comput.Pract.Exp.2022,34,e7403.[CrossRef]
7. Shuvo,S.S.;Symum,H.;Ahmed,M.R.;Yilmaz,Y.;Zayas-Castro,J.L. Multi-ObjectiveReinforcementLearningBasedHealthcare
ExpansionPlanningConsideringPandemicEvents. IEEEJ.Biomed.HealthInform.2022,1–11.[CrossRef]
8. Faria,R.D.R.;Capron,B.D.O.;Secchi,A.R.;deSouza,M.B. WhereReinforcementLearningMeetsProcessControl:Reviewand
Guidelines. Processes2022,10,2311.[CrossRef]
9. Nian,R.;Liu,J.;Huang,B. AreviewOnreinforcementlearning: Introductionandapplicationsinindustrialprocesscontrol.
Comput.Chem.Eng.2020,139,106886.[CrossRef]
10. Shaqour, A.; Hagishima, A. SystematicReviewonDeepReinforcementLearning-BasedEnergyManagementforDifferent
BuildingTypes. Energies2022,15,8663.[CrossRef]
11. Liu,H.;Cai,K.;Li,P.;Qian,C.;Zhao,P.;Wu,X. REDRL:Areview-enhancedDeepReinforcementLearningmodelforinteractive
recommendation. ExpertSyst.Appl.2022,213,118926.[CrossRef]
12. Sewak,M.;Sahay,S.K.;Rathore,H.DeepReinforcementLearningintheAdvancedCybersecurityThreatDetectionandProtection.
Inf.Syst.Front.2022,25,589–611.[CrossRef]
13. Cai, P.; Wang, H.; Huang, H.; Liu, Y.; Liu, M. Vision-BasedAutonomousCarRacingUsingDeepImitativeReinforcement
Learning. IEEERobot.Autom.Lett.2021,6,7262–7269.[CrossRef]
14. SureshBabu,V.;Behl,M. ThreadingtheNeedle—OvertakingFrameworkforMulti-agentAutonomousRacing. SAEInt. J.
Connect.Autom.Veh.2022,5,33–43.[CrossRef]
15. Amini,A.;Gilitschenski,I.;Phillips,J.;Moseyko,J.;Banerjee,R.;Karaman,S.;Rus,D. LearningRobustControlPoliciesfor
End-to-EndAutonomousDrivingfromData-DrivenSimulation. IEEERobot.Autom.Lett.2020,5,1143–1150.[CrossRef]
16. Walker,V.;Vanegas,F.;Gonzalez,F. NanoMap: AGPU-AcceleratedOpenVDB-BasedMappingandSimulationPackagefor
RoboticAgents. RemoteSens.2022,14,5463.[CrossRef]
17. Woz´niak,M.;Zielonka,A.;Sikora,A. Drivingsupportbytype-2fuzzylogiccontrolmodel. ExpertSyst.Appl.2022,207,117798.
[CrossRef]
18. Wei,W.;Gao,F.;Scherer,R.;Damasevicius,R.;Połap,D. Designandimplementationofautonomouspathplanningforintelligent
vehicle. J.InternetTechnol.2021,22,957–965.[CrossRef]
19. Zagradjanin, N.; Rodic, A.; Pamucar, D.; Pavkovic, B. Cloud-based multi-robot path planning in complex and crowded
environmentusingfuzzylogicandonlinelearning. Inf.Technol.Control2021,50,357–374.[CrossRef]
20. Mehmood,A.;Shaikh,I.U.H.;Ali,A. Applicationofdeepreinforcementlearningtrackingcontrolof3wdomnidirectionalmobile
robot. Inf.Technol.Control2021,50,507–521.[CrossRef]
21. Xuhui,B.;Rui,H.;Yanling,Y.;Wei,Y.;Jiahao,G.;Xinghe,M. Distributediterativelearningformationcontrolfornonholonomic
multiplewheeledmobilerobotswithchannelnoise. Inf.Technol.Control2021,50,588–600.
22. Bathla,G.;Bhadane,K.;Singh,R.K.;Kumar,R.;Aluvalu,R.;Krishnamurthi,R.;Kumar,A.;Thakur,R.N.;Basheer,S. Autonomous
VehiclesandIntelligentAutomation:Applications,ChallengesandOpportunities. Mob.Inf.Syst.2022,2022,7632892.[CrossRef]
23. Wang, J.; Xu, Z.; Zheng, X.; Liu, Z. A Fuzzy Logic Path Planning Algorithm Based on Geometric Landmarks and Kinetic
Constraints. Inf.Technol.Control2022,51,499–514.[CrossRef]
24. Luneckas,M.;Luneckas,T.;Udris,D.;Plonis,D.;Maskeliunas,R.;Damasevicius,R. Energy-efficientwalkingoverirregular
terrain:Acaseofhexapodrobot. Metrol.Meas.Syst.2019,26,645–660.

Information2023,14,290 21of22
25. Luneckas,M.;Luneckas,T.;Udris,D.;Plonis,D.;Maskeliu¯nas,R.;Damaševicˇius,R. Ahybridtactilesensor-basedobstacle
overcomingmethodforhexapodwalkingrobots. Intell.Serv.Robot.2021,14,9–24.[CrossRef]
26. Ayawli,B.B.K.;Mei,X.;Shen,M.;Appiah,A.Y.;Kyeremeh,F. OptimizedRRT-A*pathplanningmethodformobilerobotsin
partiallyknownenvironment. Inf.Technol.Control2019,48,179–194.[CrossRef]
27. Palacios,F.M.;Quesada,E.S.E.;Sanahuja,G.;Salazar,S.;Salazar,O.G.;Carrillo,L.R.G. Testbedforapplicationsofheterogeneous
unmannedvehicles. Int.J.Adv.Robot.Syst.2017,14,172988141668711.[CrossRef]
28. Herman,J.;Francis,J.;Ganju,S.;Chen,B.;Koul,A.;Gupta,A.;Skabelkin,A.;Zhukov,I.;Kumskoy,M.;Nyberg,E. Learn-to-Race:
AMultimodalControlEnvironmentforAutonomousRacing. InProceedingsofthe2021IEEE/CVFInternationalConferenceon
ComputerVision(ICCV),Montreal,BC,Canada,11–17October2021.[CrossRef]
29. Almón-Manzano,L.;Pastor-Vargas,R.;Troncoso,J.M.C. DeepReinforcementLearninginAgents’Training:UnityML-Agents;Lecture
NotesinComputerScience(includingsubseriesLectureNotesinArtificialIntelligenceandLectureNotesinBioinformatics);
Springer:Berlin,Germany,2022;Volume13259LNCS,pp.391–400.
30. Yasufuku,K.;Katou,G.;Shoman,S. Gameengine(Unity,UnrealEngine). KyokaiJohoImejiZasshi/J.Inst.ImageInf.Telev.Eng.
2017,71,353–357.[CrossRef]
31. S¸erban,G. ANewProgrammingInterfaceforReinforcementLearningSimulations. InAdvancesinSoftComputing;Springer:
Berlin/Heidelberg,Germany,2005;pp.481–485. [CrossRef]
32. RamezaniDooraki,A.;Lee,D.J. Anend-to-enddeepreinforcementlearning-basedintelligentagentcapableofautonomous
explorationinunknownenvironments. Sensors2018,18,3575.[CrossRef]
33. Urrea,C.;Garrido,F.;Kern,J. Designandimplementationofintelligentagenttrainingsystemsforvirtualvehicles. Sensors2021,
21,492.[CrossRef]
34. Juliani,A.;Berges,V.P.;Teng,E.;Cohen,A.;Harper,J.;Elion,C.;Goy,C.;Gao,Y.;Henry,H.;Mattar,M.;etal. Unity:Ageneral
platformforintelligentagents. arXiv2018,arXiv:1809.02627.
35. Mnih, V.; Kavukcuoglu, K.; Silver, D.; Rusu, A.A.; Veness, J.; Bellemare, M.G.; Graves, A.; Riedmiller, M.; Fidjeland, A.K.;
Ostrovski,G.;etal. Human-levelcontrolthroughdeepreinforcementlearning. Nature2015,518,529–533.[CrossRef][PubMed]
36. Bojarski,M.;DelTesta,D.;Dworakowski,D.;Firner,B.;Flepp,B.;Goyal,P.;Jackel,L.D.;Monfort,M.;Muller,U.;Zhang,J.;etal.
EndtoEndLearningforSelf-DrivingCars.arXiv2016,arXiv:1604.07316.
37. Lowe,R.;Wu,Y.;Tamar,A.;Harb,J.;Abbeel,P.;Mordatch,I. Multi-AgentActor-CriticforMixedCooperative-Competitive
Environments. InProceedingsofthe31stInternationalConferenceonNeuralInformationProcessingSystems,LongBeach,CA,
USA,4–9December2017;CurranAssociatesInc.:RedHook,NY,USA,2017;NIPS’17,pp.6382–6393.
38. Guckiran,K.;Bolat,B. AutonomousCarRacinginSimulationEnvironmentUsingDeepReinforcementLearning. InProceedings
ofthe2019InnovationsinIntelligentSystemsandApplicationsConference(ASYU),Izmir,Turkey,31October–2November2019.
[CrossRef]
39. Barto,A.G.;Sutton,R.S.;Anderson,C.W. Neuronlikeadaptiveelementsthatcansolvedifficultlearningcontrolproblems. IEEE
Trans.Syst.ManCybern.1983,SMC-13,834–846.[CrossRef]
40. Bhattacharyya,R.P.;Phillips,D.J.;Wulfe,B.;Morton,J.;Kuefler,A.;Kochenderfer,M.J. Multi-AgentImitationLearningfor
DrivingSimulation. InProceedingsofthe2018IEEE/RSJInternationalConferenceonIntelligentRobotsandSystems(IROS),
Madrid,Spain,1–5October2018.[CrossRef]
41. Palanisamy,P. Multi-AgentConnectedAutonomousDrivingusingDeepReinforcementLearning. InProceedingsofthe2020
InternationalJointConferenceonNeuralNetworks(IJCNN),Glasgow,UK,19–24July2020. [CrossRef]
42. Chen,S.;Leng,Y.;Labi,S. Adeeplearningalgorithmforsimulatingautonomousdrivingconsideringpriorknowledgeand
temporalinformation. Comput.-AidedCiv.Infrastruct.Eng.2019,35,305–321.[CrossRef]
43. Almasi,P.;Moni,R.;Gyires-Toth,B. RobustReinforcementLearning-basedAutonomousDrivingAgentforSimulationandReal
World. InProceedingsofthe2020InternationalJointConferenceonNeuralNetworks(IJCNN),Glasgow,UK,19–24July2020.
[CrossRef]
44. Ma,G.;Wang,Z.;Yuan,X.;Zhou,F. ImprovingModel-BasedDeepReinforcementLearningwithLearningDegreeNetworksand
ItsApplicationinRobotControl. J.Robot.2022,2022,7169594.[CrossRef]
45. Onishi, T.; Motoyoshi, T.; Suga, Y.; Mori, H.; Ogata, T. End-to-endLearningMethodforSelf-DrivingCarswithTrajectory
RecoveryUsingaPath-followingFunction. InProceedingsofthe2019InternationalJointConferenceonNeuralNetworks
(IJCNN),Budapest,Hungary,14–19July2019.[CrossRef]
46. Schulman, J.; Wolski, F.; Dhariwal, P.; Radford, A.; Klimov, O. Proximal Policy Optimization Algorithms. arXiv 2017,
arXiv:1707.06347.
47. Cohen,A.;Teng,E.;Berges,V.P.;Dong,R.P.;Henry,H.;Mattar,M.;Zook,A.;Ganguly,S. OntheUseandMisuseofAbsorbing
StatesinMulti-agentReinforcementLearning.arXiv2021,arXiv:2111.05992.
48. Yu,C.;Velu,A.;Vinitsky,E.;Gao,J.;Wang,Y.;Bayen,A.;Wu,Y. TheSurprisingEffectivenessofPPOinCooperative,Multi-Agent
Games.arXiv2021,arXiv:2103.01955.
49. Reichler,J.A.; Harris,H.D.; Savchenko,M.A. OnlineParallelBoosting. InProceedingsofthe19thNationalConferenceon
ArtificalIntelligence,SanJose,CA,USA,25–29July2004;AAAIPress:MenloPark,CA,USA,2004;AAAI’04,pp.366–371.
50. Tang,Z.;Luo,L.;Xie,B.;Zhu,Y.;Zhao,R.;Bi,L.;Lu,C. AutomaticSparseConnectivityLearningforNeuralNetworks.arXiv
2022,arXiv:2201.05020.

Information2023,14,290 22of22
51. Zhu, M.; Gupta, S. To prune or not to prune: Exploring the efficacy of pruning for model compression. arXiv 2017,
arXiv:1710.01878.
52. Hu,W.;Che,Z.;Liu,N.;Li,M.;Tang,J.;Zhang,C.;Wang,J. CATRO:ChannelPruningviaClass-AwareTraceRatioOptimization.
IEEETrans.NeuralNetw.Learn.Syst.2023,1–13.[CrossRef][PubMed]
53. Palacios,E.;Peláez,E. TowardstrainingswarmsforgameAI. InProceedingsofthe22ndInternationalConferenceonIntelligent
GamesandSimulation,GAME-ON2021,Aveiro,Portugal,22–24September2021;pp.27–34.
54. Kovalský,K.;Palamas,G. Neuroevolutionvs. ReinforcementLearningforTrainingNonPlayerCharactersinGames: TheCaseofa
SelfDrivingCar;LectureNotesoftheInstituteforComputerSciences,Social-InformaticsandTelecommunicationsEngineering;
Springer:Berlin/Heidelberg,Germany,2021;Volume377,pp.191–206.
55. Laskin,M.;Lee,K.;Stooke,A.;Pinto,L.;Abbeel,P.;Srinivas,A. ReinforcementLearningwithAugmentedData. arXiv2020,
arXiv:2004.14990.
Disclaimer/Publisher’s Note: The statements, opinions and data contained in all publications are solely those of the individual
author(s)andcontributor(s)andnotofMDPIand/ortheeditor(s).MDPIand/ortheeditor(s)disclaimresponsibilityforanyinjuryto
peopleorpropertyresultingfromanyideas,methods,instructionsorproductsreferredtointhecontent.

## Extracted Images

### Page 1

![page001_img001.png](img/page001_img001.png)
![page001_img002.png](img/page001_img002.png)
![page001_img003.png](img/page001_img003.png)
![page001_img004.png](img/page001_img004.png)
![page001_img005.png](img/page001_img005.png)

### Page 5

![page005_img001.jpeg](img/page005_img001.jpeg)

### Page 6

![page006_img001.jpeg](img/page006_img001.jpeg)
![page006_img002.jpeg](img/page006_img002.jpeg)

### Page 7

![page007_img001.jpeg](img/page007_img001.jpeg)

### Page 8

![page008_img001.jpeg](img/page008_img001.jpeg)

### Page 10

![page010_img001.png](img/page010_img001.png)

### Page 12

![page012_img001.jpeg](img/page012_img001.jpeg)
![page012_img002.jpeg](img/page012_img002.jpeg)
![page012_img003.jpeg](img/page012_img003.jpeg)
![page012_img004.jpeg](img/page012_img004.jpeg)

### Page 13

![page013_img001.jpeg](img/page013_img001.jpeg)
![page013_img002.jpeg](img/page013_img002.jpeg)
![page013_img003.jpeg](img/page013_img003.jpeg)
![page013_img004.jpeg](img/page013_img004.jpeg)

### Page 14

![page014_img001.jpeg](img/page014_img001.jpeg)
![page014_img002.jpeg](img/page014_img002.jpeg)

### Page 15

![page015_img001.jpeg](img/page015_img001.jpeg)
![page015_img002.jpeg](img/page015_img002.jpeg)
![page015_img003.jpeg](img/page015_img003.jpeg)
![page015_img004.jpeg](img/page015_img004.jpeg)

### Page 16

![page016_img001.jpeg](img/page016_img001.jpeg)
![page016_img002.jpeg](img/page016_img002.jpeg)
![page016_img003.jpeg](img/page016_img003.jpeg)
![page016_img004.png](img/page016_img004.png)

### Page 17

![page017_img001.png](img/page017_img001.png)
