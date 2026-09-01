# Sistema de Gestão de Colaboradores e Unidades

## **Tecnologias**
* Backend: C#.
* Front: Angular.
* DB: PostgreSQL.

## **Funcionalidades**
* Cadastro de Usuário: Os usuários devem ser cadastrados com um código único, login, senha e status (ativo ou inativo).
* Atualização de Informações de Usuários: É possível atualizar as informações de usuário, somente senha e status (ativo ou inativo).
* Listagem de Usuários: O sistema oferece a funcionalidade de listar todos os usuário cadastrados, exibindo seus login e status. Deve também permitir uma consulta apenas por status.
* Cadastro de Colaboradores: Os colaboradores devem ser cadastrados com um código único, nome e relacionados a uma unidade específica. Todo colaborador deve ter um usuário relacionado.
* Atualização de Informações de Colaboradores: É possível atualizar as informações de colaboradores, incluindo o nome e a unidade à qual estão associados.
* Remoção de Colaboradores: Os colaboradores podem ser removidos do sistema.
* Listagem de Colaboradores: O sistema oferece a funcionalidade de listar todos os colaboradores cadastrados, exibindo seus códigos, nomes e unidades associadas.
* Cadastro de Unidades: O sistema permite o cadastro de unidades, associando um ID único, um código de unidade único e um nome à unidade.
* Atualização de Informações de Unidades: As unidades podem ser inativadas, e quando inativadas não podem permitir a inclusão de novos colaboradores.
* Listagem de Unidades: O sistema deve permitir listar todas as unidades cadastradas e todos os colaboradores relacionadados.

## **Diferenciais**
* Utilização do Docker para criação do banco de dados.
* Criar autenticação via Bearer token. 

## **Requisitos**
* Desenvolver arquitetura do projeto em MVC.
* Aplicar o pattern de herança.
* Implementar portal com todas as funcionalidades.
* Deve ser possível também realizar os testes das funcionalidades via Postman ou similares.

## Passos para envio da avaliação
* Crie um fork da master para seu repositório com o seguinte nome: usuário do git e data, ex.: devrte-20231201.
* Envie link do projeto criado para o email: desenvolvedor.rte@gmail.com com o título: [RTE] - Avaliação técnica / Seu Nome
* **Após a solicitação de acesso, haverá o prazo de uma semana para entrega do projeto**
