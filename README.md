# 포트폴리오용 프로젝트

> [!NOTE]
> 계속 개발중인 프로젝트입니다.
> <br>README도 지속적으로 갱신중...

> [!IMPORTANT]
> ![Code_2024-05-15_14-33-56](https://github.com/KimLiquid/MyProject/assets/114733076/f7b9e241-61fc-4e1d-a5bd-2718d28f8beb)
> <br>안쓰는 코드 중 일부는 이런식으로 안지우고 주석으로 처리해놔서 스크립트 보기 좀 더러울 수 있음

## 사용 스크립트 

### Player가 사용하는 스크립트
![Unity_2024-05-15_15-45-26](https://github.com/KimLiquid/MyProject/assets/114733076/fec51a56-94d4-49cc-a1de-f4ec3b2589ff) 
<br>Character 안에는 매쉬 랜더러, 리깅이 들어감
<br>Camera 안에는 FPS시점과 TPS시점의 카메라가 들어감
<br>![Unity_2024-05-15_15-49-42](https://github.com/KimLiquid/MyProject/assets/114733076/9cd8693b-a3c7-4ea9-9e48-1ce14a1cf28d)
![Unity_2024-05-15_15-49-31](https://github.com/KimLiquid/MyProject/assets/114733076/18fb3eb4-f98e-4a7f-9fba-8057c3e05031)

### Enemy가 사용하는 스크립트
![Unity_2024-05-15_18-23-39](https://github.com/KimLiquid/MyProject/assets/114733076/9f3eac09-3b5b-4dee-8d35-34d44ae908b0)
<br>※ EnemyDefinition.cs는 현재 사용하지않음 (들어가보면 모든 코드가 주석처리되어있음)<br>
![Unity_2024-05-15_18-36-38](https://github.com/KimLiquid/MyProject/assets/114733076/d18a5553-de2a-4301-afc7-c1a4a7582ae7)

## 스크립트 기능 설명
> [!IMPORTANT]
> ![msedge_2024-05-17_23-01-28](https://github.com/KimLiquid/MyProject/assets/114733076/9c1ea486-3058-4afe-9452-59a22e26c698)
> <br>움짤이 안보일수도 있는데 기다리다보면 나옴

### Player쪽 기능
![Unity_2024-05-15_22-25-56](https://github.com/KimLiquid/MyProject/assets/114733076/e4fa73c0-1da0-4661-9fb1-7550c88a66e4)
<br>**컴포넌트에 카메라를 넣어놔야 함[^1]**
[^1]:안 넣어 두면 오류발생 

![Unity_2024-05-15_20-20-16](https://github.com/KimLiquid/MyProject/assets/114733076/e3157a2c-aaf9-4b1d-bfb2-fa932af6ec38)
<br>W - 앞으로 이동
<br>S - 뒤로 이동
<br>A - 왼쪽으로 이동
<br>D - 오른쪽으로 이동

![Unity_2024-05-15_20-41-40](https://github.com/KimLiquid/MyProject/assets/114733076/4e855963-7692-4ea2-8621-c49b4e9f4b51)
<br>왼쪽 쉬프트 - 대쉬
<br>왼쪽 컨트롤 - 걷기

![Unity_2024-05-15_20-44-19](https://github.com/KimLiquid/MyProject/assets/114733076/eb6747d3-3434-4323-aa6f-924d892550c6)
![Unity_2024-05-15_20-47-32](https://github.com/KimLiquid/MyProject/assets/114733076/da6e8437-567f-4e1e-aaa1-d34b11ba93fa)
<br>스페이스바 - 점프
<br>마우스 좌측 아래버튼[^2] - 구르기[^3]
[^2]:사진에서는 왼쪽 알트키를 사용
[^3]:아직 구르기에 무적은 없음, 구른 후 약간의 쿨타임이 있음

![Unity_2024-05-15_21-16-44](https://github.com/KimLiquid/MyProject/assets/114733076/13f0de75-6457-4133-9210-f2716502a68b)
<br>F5 - TPS <-> FPS 시점 변경
<br>.(점) - 커서 표시[^4]
[^4]:커서 표시중에는 카메라 회전 불가능

![Unity_2024-05-15_21-24-20](https://github.com/KimLiquid/MyProject/assets/114733076/827df6d7-f5cc-4864-8e35-da5d3ef27745)
<br>휠 위/아래 - 화면 확대/축소[^5]
[^5]:시점이 TPS일때만 확대/축소 가능

![Unity_2024-05-15_21-29-25](https://github.com/KimLiquid/MyProject/assets/114733076/b7b3b6b1-a621-43e7-8eb0-90cbe1e93ca0)
<br>탭 - 카운터 자세 토글[^6]
<br>카운터 자세에서 왼쪽/휠/오른쪽 클릭 - 상단/중단/하단 카운터[^7]
[^6]:이 상태에서는 움직이지 못함, 화면 확대/축소 불가, TPS/FPS시점 변경 불가
[^7]:아직 모션만 있고 카운터 판정은 없음

![Unity_2024-05-15_21-41-38](https://github.com/KimLiquid/MyProject/assets/114733076/5b433ce5-924e-4706-a67a-7145b9a2699a)
<br>F - 등에 있는 칼을 발도/납도[^8]
<br>발도 상태에서 왼클릭 - 공격[^9]
<br>공중에서 왼클릭 - 점프 공격
[^8]:발도 상태에서는 패링 자세 불가능, 총을 꺼낼 수 없음
[^9]:10가지 모션중 랜덤으로 재생

![Unity_2024-05-15_21-55-30](https://github.com/KimLiquid/MyProject/assets/114733076/207a974a-1c71-4e98-b433-de359a6488b0)
<br>G - 총 꺼내기/집어넣기[^10]
<br>총을 꺼낸 상태에서 우클릭 - 조준/조준해제[^11]
<br>조준 중에 왼클릭 - 총 발사[^12]
<br>**모든공격은 아직은 적에게 공격 판정이 없음**
[^10]:총을 꺼낸 상태에서는 패링 자세 불가능, 발도 할 수 없음
[^11]:조준 상태에서는 조준을 풀기전 까진 위에 행동 포함 점프/구르기/총집어넣기/화면 확대/축소 불가능, FPS상태에서도 강제로 TPS시점으로 바뀜
[^12]:아직 총알이 나가진 않음

### Enemy쪽 기능
#### 순찰 모드
![Unity_2024-05-15_23-06-29 (2)](https://github.com/KimLiquid/MyProject/assets/114733076/568a25ee-d377-4db9-989f-c04cf7deddfe)
<br>타겟을 찾을 때 까지 순찰[^13]
<br>랜덤한 각도로 돌고 벽이나 올라갈 수 없는 경사를 만나거나 **순찰 시간**<sub>설정한 범위 내의 랜덤한 시간이 지날</sub> 동안 직전함
<br>벽이나 올라갈 수 없는 경사를 만나거나 **순찰 시간**<sub>설정한 범위 내의 랜덤한 시간</sub>이 지나서 멈췄을 경우 **순찰 쿨타임**<sub>설정한 범위 내의 랜덤한 시간이 지난</sub> 이후 다시 랜덤한 각도로 돌고 직진[^14]
<br>돌기만 하고 움직이지 않을 시 **순찰 쿨타임**<sub>설정한 범위내의 랜덤한 시간</sub>에 설정한 만큼 곱하고 대기
[^13]:어그로타입이 있어서 적대일때만 추격하도록 설정할려했는데 깜빡하고 빠트림
[^14]:돌기만 하고 움직이지 않을 수 있음

#### 추격 모드
![Unity_2024-05-15_23-49-38 (2)](https://github.com/KimLiquid/MyProject/assets/114733076/c4910fd0-8358-47de-9587-9b6da114892c)
<br>감지범위 안 감지 각도 안에 타겟(플레이어) 이 들어가면 순찰모드가 끝나고 추격모드가 시작됌
<br>추격모드가 시작되면 설정한 만큼 감지범위가 늘어나고 설정한 만큼 이동속도가 곱해짐
<br>이후 타겟(플레이어) 을 추격함
<br>![Unity_2024-05-16_00-16-46 (2)](https://github.com/KimLiquid/MyProject/assets/114733076/763aecbe-e4dc-4825-92f3-258f4cce0805)
<br>아직 완벽하진않지만 장애물을 회피해서 쫓아옴
<br>감지범위 바깥으로 이탈하면 감지범위, 이동속도가 다시 원상태로 돌아오고 일정시간 가만히 있다가 추격모드가 끝나고 순찰모드가 시작됌
<br>**아직 플레이어가 근처에 있을때 공격하진 않음**

## 개발 여담
<br>이 프로젝트는 22년 10~11월 이후 쯤 부터 만들기 시작해서 진짜 가아끔씩 깨작깨작 만들다가 작년쯤 부터 약간 더 집중해서 만들기 시작함
<br>**그리고 프로젝트를 시작할때부터 최대한 에셋 스토어에 있는 다른 기능성 에셋을 쓰지않는걸 목적으로 만들기 시작함**

<br>![Taskmgr_2023-08-23_21-53-42](https://github.com/KimLiquid/MyProject/assets/114733076/42d4b9f6-1af1-4f3e-b700-1f859de782d5)
![Taskmgr_2023-08-24_22-12-14](https://github.com/KimLiquid/MyProject/assets/114733076/240b0488-f918-424c-ae79-f48958baa7a7)
![Taskmgr_2023-08-30_21-22-55](https://github.com/KimLiquid/MyProject/assets/114733076/177e908e-e306-41df-a223-32439c508d9e)
![Taskmgr_2023-08-30_21-29-01](https://github.com/KimLiquid/MyProject/assets/114733076/ce5a1b47-6534-40c3-bc70-235be36a9b0d)
![Taskmgr_2023-09-05_17-56-56](https://github.com/KimLiquid/MyProject/assets/114733076/e5268667-da07-4a3f-94a8-4d36dae32984)
<br>스크립트는 여태 Visual Studio 2022 로 만들고있다가 쓰다보니 AnyCPU가 메모리를 엄청 잡아먹어서 렉이 걸리는것도 있고 AnyCPU를 닫아놓으면
<br>![devenv_2023-08-30_21-23-26](https://github.com/KimLiquid/MyProject/assets/114733076/94494b98-6d4e-4924-b62c-a78afd717727)
![devenv_2023-08-30_21-24-35](https://github.com/KimLiquid/MyProject/assets/114733076/d88716a9-0202-41f3-87d7-0fb985e5c7f9)
<br>위와 같이 함수 위에 참조표기가 사라지는데다가 닫아놔도 꽤 무거워서
<br>![Code_2024-05-20_22-09-04](https://github.com/KimLiquid/MyProject/assets/114733076/190668da-50cf-4bf9-bd5e-aafd97267852)
<br>몇달 전에 Visual Studio Code로 갈아 탐. 사용해보니까 확실히 가벼움

<br>맨 처음 플레이어 카메라/이동 스크립트를 만들때는 [이걸 토대로](https://rito15.github.io/posts/unity-fps-tps-character/) 만들기 시작함
<br>아무튼 기본적인 이동 스크립트를 만든 이후 카운터 자세/카운터 기능 -> 발도/납도/검 공격 기능 -> 총 꺼내기/집어넣기/공격 기능 -> 구르기 기능 순서로 그냥 생각나는대로 추가 기능을 만듦
<br>아직 모든공격에 공격판정이 없는것은 적은 이후에 만들고있는중이라 적들 스크립트좀 재대로 만들어지면 이후에 넣을예정

플레이어 기능 중 카운터기능은 적의 특정 공격에 각각 상단/하단/중단 전조를 시각적으로 표기해주고 그 때 쓰면 카운터되도록 만들려는것이 목적
<br>카운터 기능을 사용 하기 전 카운터 자세를 써야한다는 점이 있지만 대충 세키로의 간파[^15]나 로스트아크의 카운터 어택[^16] 느낌으로 만들려고 함
[^15]: 적이 특정 공격을 할 때 플레이어 위에 붉은색으로 危 한자가 뜸. 이 때 찌르기 공격일 경우 앞으로 회피하면 '간파'가 됌  다만 약간 다른점이라면 세키로의 危가 뜨는 공격이 하단(점프해야됌), 찌르기(앞으로 회피해야함(타이밍이 훨씬 까다롭지만 패링 가능)), 잡기(잡히기 전에 피해야함(일부 잡기는 패링 가능)) 을 쓸때 공통적으로 나오지만 나는 상단/중단/하단 카운터 전조표기를 따로 만들예정
[^16]: 적이 특정 공격을 할 때 적이 푸른색으로 빛남. 이때 카운터 기능이 달린 스킬을 사용해서 치면 '카운터 어택'이 됌

플레이어 기능 중 총 발사 기능은 통상의 *fps 게임*<sub>총 게임</sub> 처럼 총알이 에임을 따라 날라가서 맞는형식으로 만드는것이아닌 타겟을 지정하면 그 타겟을향해 히트스캔 형식으로 총격이 날라가 타겟에게 적중하도록 만들예정 
<br>물론 해드샷 같은 개념도 없음

![msedge_2024-05-16_15-07-50](https://github.com/KimLiquid/MyProject/assets/114733076/2c698450-34ba-4220-ae77-3cea13078fdf)
<br>카운터나 검/총 공격 등등에 이펙트를 넣을려고했지만 HDRP로 프로젝트를 시작해서 그런지 에셋스토어 등에서 가져온 공격용 이펙트가 싹다 안보이거나 깨져서 여기저기서 방법 찾아보기도하고 아예 이펙트를 이쁘게 만드는방법까지 찾아보기도했다가 그냥 이후에 적들 스크립트 좀 재대로 만들어지고 좀 틀이 재대로 잡힐때 넣기로 함

![Unity_2024-05-20_23-23-30](https://github.com/KimLiquid/MyProject/assets/114733076/f9a41737-dd72-4822-8805-fb69d4c407ad)
<br>MeshRenderer의 꼭짓점들을 통해서 또는 bounds 를 구해서 충돌 판정용 캡슐콜라이더/박스콜라이더를 만들었는데 박스 크기가 너무 크게만들어지거나 그런식으로 만들어져서 애좀 많이 먹다가 
<br>어찌저찌 Matrix4x4.TRS 를 이용해서 외형과 똑같이 재대로 먹도록 만들어봤는데
<br>박스콜라이더 기준으론 지금 이동시스템에 적합하지 못한것 같아서(계속 중력값만 늘어나고, 적이 이동을 안함) 일단은 박스콜라이더를 안쓰고 캡슐콜라이더만 쓰도록 타협을 봄

![Unity_2024-04-15_23-19-54](https://github.com/KimLiquid/MyProject/assets/114733076/9aad880b-a879-46ef-8a48-a2ce959de7f7)
<br>적의 타겟(플레이어)을 찾은 이후의 추적은 navmesh를 아예 안쓰고 A* 길찾기 알고리즘을 학습하고 사용할려고했으나 맵이 평평하지않고 경사져있기때문에
<br>![msedge_2024-05-17_22-18-34](https://github.com/KimLiquid/MyProject/assets/114733076/e3b3437c-6087-47e2-873e-df38b2943582)
<br>[이것 처럼](https://stonemonarch.com/2016/05/30/3d-pathfinding-in-unity-for-procedural-environments/) 그리드를 직접 만들려고 찾아보는데 영 안보여서 일단은 간단하게 앞에 장애물이 있을 때 만 우회하는식으로 추격하도록 만들었음
<br>거미는 높이가 낮아서 반지름을 0.2f 로 해놓으니 캡슐콜라이더인데도 동그랗게 만들어져서그런지 이후 타겟을 추격하는부분에서 앞에 장애물을 거의 전혀 회피 못하고있음

## 프로젝트 다운로드

### [Azure DevOps 에서 다운로드 ( 다운속도 느림 )](https://dev.azure.com/KimLiquid/_git/MyProjectDownload?anchor=%ED%94%84%EB%A1%9C%EC%A0%9D%ED%8A%B8-%EB%8B%A4%EC%9A%B4%EB%A1%9C%EB%93%9C%EC%9A%A9)

### [Google Drive 에서 다운로드 (README.md가 재대로 안보임)](https://drive.google.com/drive/folders/1zqpczYS_aKwro7mVOkdYAEtULA_FcLNe)

### 임포트 하는방법

![msedge_2024-05-21_21-52-41](https://github.com/KimLiquid/MyProject/assets/114733076/35b642f1-b37f-4dd2-bf1b-105c53438992)
<br>🔼 Azure DevOps 에서 다운받을 경우
<br>
<br>![msedge_2024-05-25_13-06-39](https://github.com/KimLiquid/MyProject/assets/114733076/b58da7d8-55da-44f0-a539-dce2fdac24f4)
<br>🔼 Google Drive 에서 다운받을 경우

<br>일단 위 사진들 처럼 ExportMyProject.unitypackage 를 다운로드

![Unity_Hub_2024-05-21_23-08-07](https://github.com/KimLiquid/MyProject/assets/114733076/34bc4822-49fa-4009-bd0c-b662a5b1c0e3)
<br>High Definition 3D 템플릿으로 프로젝트 새로 생성
<br>**(에디터 버전 2022.3.22f1 권장)**

![Unity_2024-05-21_22-36-35](https://github.com/KimLiquid/MyProject/assets/114733076/70d7e6be-6893-4503-bb5b-54ae1737ffe4)
<br>이후 다운받은 ExportMyProject.unitypackage 를 사진처럼 프로젝트에 드레그하거나 
<br>ExportEyProject.unitypackage 를 프로젝트 안에 넣고 실행  

![Unity_2024-05-21_22-26-29](https://github.com/KimLiquid/MyProject/assets/114733076/b2eeef12-9f89-49e0-b521-e13e56eb029f)
<br>![Unity_2024-05-21_22-28-36](https://github.com/KimLiquid/MyProject/assets/114733076/0a71cebf-1e18-4074-99db-43a5f0169a57)
<br>![Unity_2024-05-21_22-28-50](https://github.com/KimLiquid/MyProject/assets/114733076/ee20197e-1dc2-43e5-972d-6cac47406fc4)
<br>위에 사진과 같이 임포트

![Unity_2024-05-21_23-15-49](https://github.com/KimLiquid/MyProject/assets/114733076/eed0643a-f3fa-446a-8646-4f1ff6a1d802)
<br>임포트 완료 시 Assets/Scenes 안에 Field를 실행 (기존 기본씬은 닫기)

![Unity_2024-05-21_19-50-45](https://github.com/KimLiquid/MyProject/assets/114733076/7bb6f28c-fd65-4a96-9335-dda2e2712dd7)
<br>ExportMyProject.unitypackage 를 임포트 했으면 아마 시작 시 윗 사진처럼 2개의 스크립트가 없다고 나올텐데 ProBuilder로 만든거라 그냥 무시하시면 됌