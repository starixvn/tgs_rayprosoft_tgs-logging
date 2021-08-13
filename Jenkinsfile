pipeline {
    agent any
	
	environment {
		Registry = "registry.gitlab.com/rayprosoft/turnkey-gaming-system/"
		BuildConfiguration = 'Debug'
	}

    stages {
		stage('Prepare') {
            steps {
                sh "echo 'Branch: $GIT_BRANCH'";

                script {
                    switch(env.GIT_BRANCH)
                    {
                        case 'origin/main':
                            env.kube_ns = "prod";
                            env.k8s_config = 'stag-k8s-config';
                            break;
                        case 'origin/release':
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
				sh 'dotnet test -c $BuildConfiguration --logger trx --no-build'
			}
		}
		
		stage('Push packages') {
			when {
                expression {
                    return env.kube_ns == 'dev';
                }
            }

			steps {
				sh 'dotnet pack -c $BuildConfiguration "Tgs.Logging/Tgs.Logging.csproj" --no-build'
				sh 'dotnet nuget push "Tgs.Logging/bin/$BuildConfiguration/*.nupkg" --source "raypro" --skip-duplicate'
			}
		}

		stage('Building log-api image') {
            environment {
                ImageName="${Registry}log-api"
            }
			steps {
                sh 'dotnet build "src/Apis/LogApi" -c $BuildConfiguration -t:ContainerBuild -p:DockerfileTag="$ImageName:${TAG}" --no-restore '
                sh 'docker push $ImageName:${TAG}'
			}
        }

		stage('Deploy log-api') { 
            environment {
                ImageName="${Registry}log-api"
            }
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
	}

	post {
		success {
             sendTelegram("Pipeline: ${currentBuild.fullDisplayName} OK!\nDeployed Components: ${deployedComponents} \n${currentBuild.getBuildCauses()[0].shortDescription}\n")
        }
        failure {
            sendTelegram("<b>Failed Pipeline:</b> ${currentBuild.fullDisplayName} \n Deploy Components: ${deployedComponents} \n Something is wrong with ${env.BUILD_URL}/console\n${currentBuild.getBuildCauses()[0].shortDescription}\n")
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

def sendTelegramHTML(message) {
    def encodedMessage = URLEncoder.encode(message, "UTF-8")

    withCredentials([string(credentialsId: "telegramToken", variable: "TOKEN"), string(credentialsId: "telegramChatId", variable: "CHAT_ID")]) {

        response = httpRequest (consoleLogResponseBody: true,
                contentType: 'APPLICATION_JSON',
                httpMode: 'POST',
                requestBody: "parse_mode=HTML&text=${encodedMessage}&chat_id=${CHAT_ID}",
                url: "https://api.telegram.org/bot${TOKEN}/sendMessage",
                validResponseCodes: '200')
        return response
    }
}
