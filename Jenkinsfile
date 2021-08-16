def message = "";
pipeline {
    agent any
    
    environment {
        ImageName = "registry.gitlab.com/rayprosoft/turnkey-gaming-system/log-api"
        BuildConfiguration = 'Debug'
        ImageTagUrl = "https://gitlab.com/api/v4/projects/20478010/registry/repositories/2154485/tags"
    }

    stages {
        stage('Prepare') {
            steps {
                sh "echo 'Branch: $GIT_BRANCH'";

                script {
                    switch(env.GIT_BRANCH)
                    {
                        case ['main', 'origin/main']:
                            env.kube_ns = "prod";
                            env.k8s_config = 'stag-k8s-config';
                            break;
                        case ['release', 'origin/release']:
                            env.kube_ns = "staging";
                            env.k8s_config = 'stag-k8s-config';
                            break;
                        default:
                            env.kube_ns = "dev";
                            env.k8s_config='k8s-kube-config' 
                            break;
                    }
                }

                configFileProvider([
                    configFile(
                        fileId: 'tgs.' + env.kube_ns , 
                        variable: 'tgsconfig')]) {
                    script {
                        def envItems = readProperties file: "$tgsconfig";

                        envItems.each { key, value -> 
                            env."tgs_${key}" = value;
                        }

                        env.TAG = env.kube_ns + '-' + sh(script: "echo `date +%Y%m%d%H%M`", returnStdout: true).trim()
                        message += "\nBranch: ${env.GIT_BRANCH}" 
                        message += "\nTag: ${env.TAG}"
                        message += "\nEnv: ${env.kube_ns}"
                    }
                }

                sh "echo tag: $TAG"
            }
        }

        stage('Build') {
            steps {
                sh 'dotnet build -c $BuildConfiguration'
            }
        }

        stage('Test') {
            steps {
                sh 'dotnet test -c $BuildConfiguration -l trx --no-build'
            }
        }
        
        stage('Push packages') {
            when {
                expression {
                    return env.kube_ns == 'dev';
                }
            }

            steps {
                sh 'dotnet pack -c $BuildConfiguration "Tgs.Logging/Tgs.Logging.csproj" --no-restore'
                sh 'dotnet nuget push "Tgs.Logging/bin/$BuildConfiguration/*.nupkg" --source "raypro" --skip-duplicate'
            }
        }

        stage('Building docker image') {
            steps {
                sh 'dotnet build "LogApi" -c $BuildConfiguration -t:ContainerBuild -p:DockerfileTag="$ImageName:${TAG}" --no-restore '
            }
        }

        stage('Push docker image') {
            steps {
                sh 'docker push $ImageName:${TAG}'
            }
        }

        stage('Deploy log-api') { 
            steps {
                withKubeConfig([credentialsId: env.k8s_config]) {
                    sh '''
                        helm template deployment/k8s/log-api \
                            --set image.tag=${TAG} \
                            --set namespace=${kube_ns} \
                            --set connectionStrings.tgs="$tgs_tgs" \
                            --set connectionStrings.redis="$tgs_redis" \
                            --set nats.urls="$tgs_nats" \
                            --set aspNetCoreEnvironment="$tgs_aspNetCoreEnvironment" \
                            --set notifyPath="$tgs_notifyPath"

                        helm template deployment/k8s/log-api \
                            --set image.tag=${TAG} \
                            --set namespace=${kube_ns} \
                            --set connectionStrings.tgs="$tgs_tgs" \
                            --set connectionStrings.redis="$tgs_redis" \
                            --set nats.urls="$tgs_nats" \
                            --set aspNetCoreEnvironment="$tgs_aspNetCoreEnvironment" \
                            --set notifyPath="$tgs_notifyPath" \
                        | kubectl -n ${kube_ns} apply -f -
                    '''
                }
            }
        }

        stage('Add Tag') {
            when {
                expression {
                    return env.kube_ns == 'prod';
                }
            }		
            steps {
                sshagent(['ssh-jenkins']) {
                    sh "git tag -a ${TAG} -m ${TAG}"
                    sh "git push ${GIT_URL} ${TAG} HEAD:${GIT_BRANCH}"
                }
            }
        }

        stage('Remove Image Tags') {					
            steps {	
                withCredentials([string(credentialsId: "GitLabToken", variable: "GitLabToken")]) {
                    sh "curl --request DELETE --data \'name_regex_delete=.*\' --data \'keep_n=10\' --header \'PRIVATE-TOKEN: ${GitLabToken}\' ${ImageTagUrl}"
                }
            }
        }
    }

    post {
        success {
             sendTelegram("Pipeline: ${currentBuild.fullDisplayName} OK! ${message} \n${currentBuild.getBuildCauses()[0].shortDescription}\n")
        }
        failure {
            sendTelegram("<b>Failed Pipeline:</b> ${currentBuild.fullDisplayName} Something is wrong with ${env.BUILD_URL}/console\n${currentBuild.getBuildCauses()[0].shortDescription}\n")
        }

        always {
            step([$class: 'MSTestPublisher', testResultsFile:"**/*.trx", failOnError: false, keepLongStdio: true])
        }
    }
}

def sendTelegram(message) {
    def encodedMessage = URLEncoder.encode(message, "UTF-8")

    withCredentials([string(credentialsId: "telegramToken", variable: "TOKEN"), string(credentialsId: "telegramChatId", variable: "CHAT_ID")]) {

        response = httpRequest (consoleLogResponseBody: true,
                contentType: 'APPLICATION_JSON',
                httpMode: 'GET',
                url: "https://api.telegram.org/bot${TOKEN}/sendMessage?text=${encodedMessage}&chat_id=${CHAT_ID}",
                validResponseCodes: '200')
        return response
    }
}
